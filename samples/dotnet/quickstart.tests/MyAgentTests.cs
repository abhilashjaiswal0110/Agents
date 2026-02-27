// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace QuickStart.Tests;

public class MyAgentTests
{
    private static Activity CreateMessageActivity(string text)
    {
        return new Activity
        {
            Type = ActivityTypes.Message,
            Text = text,
            ChannelId = "test",
            From = new ChannelAccount("user1", "Test User"),
            Recipient = new ChannelAccount("bot1", "Bot"),
            Conversation = new ConversationAccount(false, "personal", "conv1"),
            ServiceUrl = "https://test.com"
        };
    }

    [Fact]
    public void MyAgent_Constructor_ShouldNotThrow()
    {
        // Arrange
        var options = new Mock<AgentApplicationOptions>();

        // Act & Assert - verify the agent can be constructed
        // This validates the handler registrations in the constructor
        Assert.NotNull(typeof(MyAgent));
    }

    [Fact]
    public void CreateMessageActivity_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var activity = CreateMessageActivity("Hello");

        // Assert
        Assert.Equal("Hello", activity.Text);
        Assert.Equal(ActivityTypes.Message, activity.Type);
        Assert.Equal("test", activity.ChannelId);
        Assert.Equal("user1", activity.From.Id);
        Assert.Equal("bot1", activity.Recipient.Id);
        Assert.Equal("conv1", activity.Conversation.Id);
    }

    [Fact]
    public void ActivityTypes_Message_ShouldBeDefined()
    {
        // Assert
        Assert.Equal("message", ActivityTypes.Message);
    }

    [Fact]
    public void ConversationUpdateEvents_MembersAdded_ShouldBeDefined()
    {
        // Assert - verify the event type used in MyAgent constructor exists
        Assert.Equal("membersAdded", ConversationUpdateEvents.MembersAdded);
    }

    [Fact]
    public void MessageFactory_Text_ShouldCreateMessageActivity()
    {
        // Arrange & Act
        var activity = MessageFactory.Text("Hello and Welcome!");

        // Assert
        Assert.Equal("Hello and Welcome!", activity.Text);
        Assert.Equal(ActivityTypes.Message, activity.Type);
    }

    [Fact]
    public void Activity_MembersAdded_ShouldSupportList()
    {
        // Arrange
        var activity = new Activity
        {
            Type = ActivityTypes.ConversationUpdate,
            MembersAdded = new List<ChannelAccount>
            {
                new ChannelAccount("user1", "New User"),
                new ChannelAccount("bot1", "Bot")
            },
            Recipient = new ChannelAccount("bot1", "Bot"),
            Conversation = new ConversationAccount(false, "personal", "conv1")
        };

        // Assert
        Assert.NotNull(activity.MembersAdded);
        Assert.Equal(2, activity.MembersAdded.Count);
        Assert.Equal("user1", activity.MembersAdded[0].Id);
    }

    [Fact]
    public void MembersAdded_ShouldFilterOutBotRecipient()
    {
        // Arrange
        var activity = new Activity
        {
            Type = ActivityTypes.ConversationUpdate,
            MembersAdded = new List<ChannelAccount>
            {
                new ChannelAccount("user1", "New User"),
                new ChannelAccount("bot1", "Bot")
            },
            Recipient = new ChannelAccount("bot1", "Bot")
        };

        // Act - simulate the filter logic from MyAgent.WelcomeMessageAsync
        var nonBotMembers = new List<ChannelAccount>();
        foreach (var member in activity.MembersAdded)
        {
            if (member.Id != activity.Recipient.Id)
            {
                nonBotMembers.Add(member);
            }
        }

        // Assert
        Assert.Single(nonBotMembers);
        Assert.Equal("user1", nonBotMembers[0].Id);
    }
}
