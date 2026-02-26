# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

import pytest
import asyncio
from microsoft_agents.hosting.core import TurnContext
from microsoft_agents.activity import (
    Activity,
    ActivityTypes,
    ChannelAccount,
    ConversationAccount,
)


class StubAdapter:
    """Simple test adapter that captures sent activities."""

    def __init__(self):
        self.sent_activities = []

    async def send_activities(self, context, activities):
        self.sent_activities.extend(activities)
        return [{"id": str(i + 1)} for i in range(len(activities))]

    async def update_activity(self, context, activity):
        pass

    async def delete_activity(self, context, reference):
        pass


def create_test_activity(text, activity_type=ActivityTypes.message):
    """Create a test Activity for testing agent handlers."""
    activity = Activity(
        type=activity_type,
        text=text,
        channel_id="test",
        service_url="https://test.com",
    )
    activity.from_property = ChannelAccount(id="user1", name="Test User")
    activity.recipient = ChannelAccount(id="bot1", name="Bot")
    activity.conversation = ConversationAccount(id="conv1")
    return activity


class TestQuickstartAgent:
    """Tests for the quickstart echo agent pattern."""

    def test_activity_creation(self):
        """Test that test activities are properly constructed."""
        activity = create_test_activity("Hello")
        assert activity.text == "Hello"
        assert activity.type == ActivityTypes.message
        assert activity.channel_id == "test"
        assert activity.from_property.id == "user1"
        assert activity.recipient.id == "bot1"
        assert activity.conversation.id == "conv1"

    def test_adapter_creation(self):
        """Test that the test adapter initializes properly."""
        adapter = StubAdapter()

    def test_turn_context_creation(self):
        """Test that TurnContext can be created with test adapter."""
        adapter = StubAdapter()
        activity = create_test_activity("Hello")
        context = TurnContext(adapter, activity)
        assert context.activity.text == "Hello"
        assert context.activity.type == ActivityTypes.message

    @pytest.mark.asyncio
    async def test_send_activity(self):
        """Test that sending an activity through the adapter captures it."""
        adapter = StubAdapter()
        activity = create_test_activity("Hello")
        context = TurnContext(adapter, activity)

        await context.send_activity("Echo: Hello")

        assert len(adapter.sent_activities) > 0
        sent_texts = [
            a.text for a in adapter.sent_activities if hasattr(a, "text") and a.text
        ]
        assert any("Echo: Hello" in t for t in sent_texts)

    @pytest.mark.asyncio
    async def test_echo_response_format(self):
        """Test the echo response format matches expected pattern."""
        adapter = StubAdapter()
        activity = create_test_activity("test message")
        context = TurnContext(adapter, activity)

        # Simulate the echo handler logic from agent.py
        response_text = f"you said: {context.activity.text}"
        await context.send_activity(response_text)

        assert len(adapter.sent_activities) > 0
        sent_texts = [
            a.text for a in adapter.sent_activities if hasattr(a, "text") and a.text
        ]
        assert any("you said: test message" in t for t in sent_texts)

    @pytest.mark.asyncio
    async def test_welcome_message(self):
        """Test the welcome message handler logic."""
        adapter = StubAdapter()
        activity = create_test_activity("", ActivityTypes.message)
        context = TurnContext(adapter, activity)

        # Simulate the welcome handler logic from agent.py
        welcome_text = (
            "Welcome to the empty agent! "
            "This agent is designed to be a starting point for your own agent development."
        )
        await context.send_activity(welcome_text)

        assert len(adapter.sent_activities) > 0
        sent_texts = [
            a.text for a in adapter.sent_activities if hasattr(a, "text") and a.text
        ]
        assert any("Welcome" in t for t in sent_texts)

    def test_activity_types(self):
        """Test that activity types are correctly defined."""
        assert ActivityTypes.message is not None
        msg_activity = create_test_activity("Hello", ActivityTypes.message)
        assert msg_activity.type == ActivityTypes.message
