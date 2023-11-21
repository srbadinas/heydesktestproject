using Heydesk.Commons.Helpers;
using Heydesk.Entities.Models;
using Heydesk.Entities.Objects;
using Heydesk.Portal.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using Conversation = Heydesk.Entities.Models.Conversation;

namespace Heydesk.Portal.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {

        public async Task<IActionResult> Index()
        {
            ChatViewModel model = new ChatViewModel();
            var userId = User.Claims.Where(c => c.Type == "UserId").FirstOrDefault()?.Value;
            if (long.TryParse(userId, out long result))
                model.LoggedInUser.Id = result;

            try
            {
                var response = await ApiHelpers.PostRequest("/Conversations/GetByUserId", model.LoggedInUser.Id);

                if (!response.IsSuccessStatusCode) {
                    model.Message = "Bad request";
                    return View(model);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                model.Conversations = JsonConvert.DeserializeObject<List<Conversation>>(responseContent);
            }
            catch (Exception ex)
            {
                model.Message = ex.Message;
                return View(model);
            }

            return View(model);
        }

        public async Task<IActionResult> Conversation(long? conversationId)
        {
            ChatViewModel model = new ChatViewModel();

            var userId = User.Claims.Where(c => c.Type == "UserId").FirstOrDefault()?.Value;

            if (long.TryParse(userId, out long result))
                model.LoggedInUser.Id = result;

            try
            {
                if (!conversationId.HasValue)
                {
                    var response = await ApiHelpers.PostRequest("/Conversations/Add", model.LoggedInUser.Id);

                    if (!response.IsSuccessStatusCode)
                    {
                        model.Message = "Bad request";
                        return View(model);
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    conversationId = JsonConvert.DeserializeObject<long>(responseContent);

                    return RedirectToAction("Conversation", new { conversationId = conversationId });
                }
                else 
                {
                    var response = await ApiHelpers.PostRequest("/Conversations/GetSingle", conversationId.Value);

                    if (!response.IsSuccessStatusCode)
                    {
                        model.Message = "Bad request";
                        return View(model);
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    model.Conversation = JsonConvert.DeserializeObject<Conversation>(responseContent);
                }

            }
            catch (Exception ex)
            {
                model.Message = ex.Message;
                return View(model);
            }

            return View(model);
        }

        public async Task<IActionResult> SendMessage(ChatViewModel model)
        {
            var userId = User.Claims.Where(c => c.Type == "UserId").FirstOrDefault()?.Value;
            if (long.TryParse(userId, out long result))
                model.LoggedInUser.Id = result;

            try
            {
                SendMessageData sendMessageData = new SendMessageData() {
                    ConversationId = model.ConversationMessage.ConversationId,
                    UserId = result,
                    Message = model.ConversationMessage.Message
                };

                var response = await ApiHelpers.PostRequest("/Conversations/SendMessage", sendMessageData);

                if (!response.IsSuccessStatusCode)
                {
                    model.Message = "Bad request";
                    return View(model);
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var conversationMessage = JsonConvert.DeserializeObject<ConversationMessage>(responseContent);
                return RedirectToAction("Conversation", new { conversationId = model.ConversationMessage.ConversationId });
            }
            catch (Exception ex)
            {
                model.Message = ex.Message;
                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            // Perform logout operations here
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Redirect to the home page or login page
            return RedirectToAction("Index", "Home");

        }


    }
}
