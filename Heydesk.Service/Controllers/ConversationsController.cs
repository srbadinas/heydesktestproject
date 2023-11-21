using Heydesk.Entities.Models;
using Heydesk.Entities.Objects;
using Heydesk.Service.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using Conversation = Heydesk.Entities.Models.Conversation;

namespace Heydesk.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationsController : ControllerBase
    {
        private readonly HeydeskContext _context;
        private readonly string OPENAI_APIKEY = "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";

        public ConversationsController(HeydeskContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("GetSingle")]
        public async Task<IActionResult> GetSingle([FromBody] long? id)
        {
            if (!id.HasValue)
            {
                return BadRequest(new { Message = "No conversation id." });
            }

            var conversation = await _context.Conversations
                .Include(c => c.ConversationMessages)
                .FirstOrDefaultAsync(c => c.Id == id.Value);

            return Ok(conversation);
        }

        [HttpPost]
        [Route("GetByUserId")]
        public async Task<IActionResult> GetByUserId([FromBody] long? userId)
        {
            List<Conversation> conversations = new List<Conversation>();

            if (!userId.HasValue)
            {
                return BadRequest(new { Message = "No user id." });
            }

            conversations = await _context.Conversations
                    .Include(c => c.ConversationMessages)
                    .Where(c => c.UserId == userId.Value)
                    .OrderByDescending(c => c.CreatedDate)
                    .Take(5)
                    .ToListAsync();

            return Ok(conversations);
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Add([FromBody] long? userId)
        {
            Conversation conversation = new Conversation();

            try
            {
                if (!userId.HasValue)
                {
                    return BadRequest(new { Message = "No user id." });
                }

                conversation.UserId = userId.Value;
                conversation.IsAiResponse = true;
                await _context.Conversations.AddAsync(conversation);
                await _context.SaveChangesAsync();

                OpenAIAPI api = new OpenAIAPI(OPENAI_APIKEY);
                var results = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
                {
                    Model = Model.ChatGPTTurbo,
                    Temperature = 0.1,
                    MaxTokens = 50,
                    Messages = new ChatMessage[] {
                    new ChatMessage(ChatMessageRole.System, "You are a kind and friendly export in analyzing and extracting information from various sources. You are part of the business, so all responses MUST be in the first person plural. All your responses MUST be friendly and informal.")
                },
                });

                var reply = results.Choices[0].Message;

                ConversationMessage message = new ConversationMessage()
                {
                    ConversationId = conversation.Id,
                    SenderId = 1, // AI
                    RecipientId = userId.Value,
                    Message = reply.Content,
                };

                conversation.ConversationMessages.Add(message);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                throw new Exception(ex.Message);
            }

            return Ok(conversation.Id);
        }

        [HttpPost]
        [Route("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageData messageContent)
        {
            var conversation = await _context.Conversations
                .Include(m => m.ConversationMessages)
                .FirstOrDefaultAsync(c => c.Id == messageContent.ConversationId.Value);

            try
            {
                await using (var hubConnection = new HubConnectionBuilder()
                    .WithUrl("https://localhost:7062/ChatHub")
                    .WithAutomaticReconnect()
                    .Build())
                {
                    hubConnection.ServerTimeout = TimeSpan.FromSeconds(5);

                    await hubConnection.StartAsync();

                    if (messageContent.UserId == conversation.UserId) {
                        await hubConnection.InvokeAsync("SendMessage", conversation.Id, messageContent.Message, true);
                    }


                    if (conversation.IsAiResponse == true && messageContent.UserId == conversation.UserId)
                    {

                        var chatMessages = new List<ChatMessage>() {
                            new ChatMessage(ChatMessageRole.System, "You are a kind and friendly export in analyzing and extracting information from various sources. You are part of the business, so all responses MUST be in the first person plural. All your responses MUST be friendly and informal.")
                        };

                        foreach (var item in conversation.ConversationMessages.OrderBy(c => c.CreatedDate).ToList())
                        {
                            if (item.SenderId == 1)
                            {
                                chatMessages.Add(new ChatMessage()
                                {
                                    Role = ChatMessageRole.Assistant,
                                    Content = item.Message
                                });
                            }
                            else
                            {
                                chatMessages.Add(new ChatMessage()
                                {
                                    Role = ChatMessageRole.User,
                                    Content = item.Message
                                });
                            }
                        }

                        chatMessages.Add(new ChatMessage()
                        {
                            Role = ChatMessageRole.User,
                            Content = messageContent.Message
                        });

                        var conversationMessage = new ConversationMessage()
                        {
                            ConversationId = messageContent.ConversationId.Value,
                            SenderId = messageContent.UserId.Value,
                            RecipientId = 1,
                            Message = messageContent.Message
                        };

                        await _context.ConversationMessages.AddAsync(conversationMessage);
                        await _context.SaveChangesAsync();


                        OpenAIAPI api = new OpenAIAPI(OPENAI_APIKEY);
                        var results = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
                        {
                            Model = Model.ChatGPTTurbo,
                            Temperature = 0.1,
                            MaxTokens = 50,
                            Messages = chatMessages,
                        });

                        var reply = results.Choices[0].Message;

                        ConversationMessage message = new ConversationMessage()
                        {
                            ConversationId = conversation.Id,
                            SenderId = 1, // AI
                            RecipientId = messageContent.UserId.Value,
                            Message = reply.Content,
                        };

                        conversation.ConversationMessages.Add(message);
                        await _context.SaveChangesAsync();

                        await hubConnection.InvokeAsync("SendMessage", conversation.Id, reply.Content, false);
                    }
                    else
                    {
                        conversation.IsAiResponse = false;
                        _context.Conversations.Update(conversation);
                        await _context.SaveChangesAsync();

                        var conversationMessage = new ConversationMessage()
                        {
                            ConversationId = messageContent.ConversationId.Value,
                            SenderId = messageContent.UserId.Value,
                            RecipientId = conversation.UserId,
                            Message = messageContent.Message
                        };

                        await _context.ConversationMessages.AddAsync(conversationMessage);
                        await _context.SaveChangesAsync();

                        await hubConnection.InvokeAsync("SendMessage", conversation.Id, messageContent.Message, false);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }


    }
}
