// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import { describe, it, expect, beforeEach } from 'vitest'
import {
  TurnState,
  MemoryStorage,
  TurnContext,
  AgentApplication,
  BaseAdapter,
  AttachmentDownloader
} from '@microsoft/agents-hosting'
import { Activity, ActivityTypes } from '@microsoft/agents-activity'
import type { ResourceResponse } from '@microsoft/agents-hosting'
import type { ConversationReference } from '@microsoft/agents-activity'
import type { JwtPayload } from 'jsonwebtoken'

// Test adapter that captures sent activities
class TestAdapter extends BaseAdapter {
  public sentActivities: Activity[] = []

  async sendActivities (_context: TurnContext, activities: Activity[]): Promise<ResourceResponse[]> {
    this.sentActivities.push(...activities)
    return activities.map((_, i) => ({ id: String(i + 1) }))
  }

  async updateActivity (): Promise<void> {}
  async deleteActivity (): Promise<void> {}
  async continueConversation (
    _botAppIdOrIdentity: string | JwtPayload,
    _reference: Partial<ConversationReference>,
    _logic: (ctx: TurnContext) => Promise<void>
  ): Promise<void> {}

  async uploadAttachment (): Promise<ResourceResponse> { return { id: '1' } }
  async getAttachmentInfo (): Promise<{ name: string }> { return { name: '' } }
  async getAttachment (): Promise<NodeJS.ReadableStream> { return null as unknown as NodeJS.ReadableStream }
}

// Conversation state interface matching the sample
interface ConversationState {
  count: number;
}
type ApplicationTurnState = TurnState<ConversationState>

function createTestActivity (text: string, type: string = ActivityTypes.Message): Activity {
  return Activity.fromObject({
    type,
    text,
    channelId: 'test',
    from: { id: 'user1', name: 'Test User' },
    recipient: { id: 'bot1', name: 'Bot' },
    conversation: { id: 'conv1' },
    serviceUrl: 'https://test.com'
  })
}

describe('Quickstart Agent', () => {
  let adapter: TestAdapter
  let agentApp: AgentApplication<ApplicationTurnState>

  beforeEach(() => {
    adapter = new TestAdapter()
    const storage = new MemoryStorage()
    const downloader = new AttachmentDownloader()

    agentApp = new AgentApplication<ApplicationTurnState>({
      storage,
      adapter,
      fileDownloaders: [downloader]
    })

    // Register handlers matching the sample
    agentApp.onConversationUpdate('membersAdded', async (context: TurnContext, _state: ApplicationTurnState) => {
      await context.sendActivity('Hello and Welcome!')
    })

    agentApp.onActivity(ActivityTypes.Message, async (context: TurnContext, state: ApplicationTurnState) => {
      let count = state.conversation.count ?? 0
      state.conversation.count = ++count
      await context.sendActivity(`[${count}] You said: ${context.activity.text}`)
    })
  })

  it('should respond to a message with echo and count', async () => {
    const activity = createTestActivity('Hello')
    const context = new TurnContext(adapter, activity)

    await agentApp.run(context)

    const textResponses = adapter.sentActivities.filter(a => a.type === ActivityTypes.Message || a.text)
    expect(textResponses.length).toBeGreaterThan(0)
    const echoResponse = textResponses.find(a => a.text?.includes('You said:'))
    expect(echoResponse).toBeDefined()
    expect(echoResponse!.text).toContain('Hello')
    expect(echoResponse!.text).toContain('[1]')
  })

  it('should increment count across messages in the same conversation', async () => {
    // First message
    const activity1 = createTestActivity('First')
    const context1 = new TurnContext(adapter, activity1)
    await agentApp.run(context1)

    // Second message
    adapter.sentActivities = []
    const activity2 = createTestActivity('Second')
    const context2 = new TurnContext(adapter, activity2)
    await agentApp.run(context2)

    const textResponses = adapter.sentActivities.filter(a => a.text?.includes('You said:'))
    expect(textResponses.length).toBeGreaterThan(0)
    expect(textResponses[0].text).toContain('[2]')
    expect(textResponses[0].text).toContain('Second')
  })

  it('should handle empty text message gracefully', async () => {
    const activity = createTestActivity('')
    const context = new TurnContext(adapter, activity)

    // Should not throw
    await agentApp.run(context)

    const textResponses = adapter.sentActivities.filter(a => a.text?.includes('You said:'))
    expect(textResponses.length).toBeGreaterThan(0)
  })
})
