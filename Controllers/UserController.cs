using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LiveChat_IlirG.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.IO;

namespace LiveChat_IlirG.Controllers
{
    /// <summary>
    /// Operations for user
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        //get the db context
        private readonly LiveChatContext DB;

        private IHubContext<ChatHub> Hub;
        public UserController(LiveChatContext db, IHubContext<ChatHub> hub)
        {
            DB = db;
            Hub = hub;
        }

        /// <summary>
        /// Connects user to the chat.
        /// </summary>
        /// /// <remarks>
        /// Sample request:
        /// 
        ///     Get api/User/AllowUserToChat
        ///     {        
        ///       "UserId": "1234",
        ///       "RecipientId": "4567",
        ///       "SecurityCode": "1111-2222-3333"        
        ///       "Message": "Hello Wrold"        
        ///     }
        /// </remarks>
        /// <param name="input"></param>        
        /// <returns>Returns true if the message has been send.</returns>
        ///  <response code="500">Return a server failure, i.e cannot connect to the database.</response>   
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Produces("application/json")]
        [HttpGet("AllowUserToChat")]
        public async Task<IActionResult> AllowUserToChatSync([FromQuery] ChatInput input)
        {
            try
            {
                //check if user exists or if the security code belongs to that one
                var user = DB.Users.Where(u => u.SecurityCode == input.SecurityCode && u.UserId == input.UserId).SingleOrDefault();
                if (user == null)
                {
                    return BadRequest("Cannot find the user");
                }


                //check if contact exists between user that is sending the message and receipment, if no add one
                var contactExists = DB.UserContacts.Where(c => c.UserId == input.UserId && c.ContactUserId == input.RecipientId).Any();
                if (!contactExists)
                {
                    await DB.UserContacts.AddAsync(new UserContacts { ContactUserId = input.RecipientId, UserId = input.UserId });
                    await DB.SaveChangesAsync();
                }
                //save the message to the db
                await DB.ChatMessages.AddAsync(new ChatMessage { Message = input.Message, Date = DateTime.Now, UserId = input.UserId, RecipientId = input.RecipientId });
                await DB.SaveChangesAsync();
                //send the message to user via signalR method               
                await Hub.Clients.User(input.UserId).SendAsync("ReceiveMessage", input.Message);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Returns the chat list.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     Get api/User/PreviousChat
        ///     {        
        ///       "UserId": "1234",
        ///       "RecipientId": "4567",
        ///       "SecurityCode": "1111-2222-3333"        
        ///       "Message": "Hello Wrold"        
        ///     }
        /// </remarks>
        /// <param name="input"></param>      
        /// <returns>Returns a list of the object type ChatResponse.</returns>
        ///  <response code="500">Return a server failure, i.e cannot connect to the database.</response>   
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Produces("application/json")]
        [HttpGet("PreviousChat")]
        public async Task<IActionResult> GetPreviousChatAsync([FromQuery] ChatInput input)
        {
            try
            {
                //validate user inputs for userId, RecipientId and SecurityCode
                if (string.IsNullOrEmpty(input.UserId))
                {
                    return BadRequest("User Id is not valid");
                }
                if (string.IsNullOrEmpty(input.RecipientId))
                {
                    return BadRequest("Recipient Id is not valid");
                }
                if (string.IsNullOrEmpty(input.SecurityCode))
                {
                    return BadRequest("Security Code is not valid");
                }
                //check if user exists or if the security code belongs to that one
                var user = DB.Users.Where(u => u.SecurityCode == input.SecurityCode && u.UserId == input.UserId).SingleOrDefault();
                if (user == null)
                {
                    return BadRequest("Cannot find the user");
                }
                //base url
                var BaseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                //get the chat list from db
                var previousChat = await DB.ChatMessages
                .Where(c => c.UserId == input.UserId && c.RecipientId == input.RecipientId)
                .OrderByDescending(c => c.Id)
                .Select(c => new ChatResponse
                {
                    Image = string.IsNullOrEmpty(c.Image) ? null : Path.Combine(BaseUrl, "Files", c.Image),
                    Message = c.Message
                }).FirstOrDefaultAsync();

                //return the chatList
                return Ok(previousChat);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Internal server error");
            }
        }

        /// <summary>
        /// Returns the user's contact list.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     Get api/User/UserContactList
        ///     {        
        ///       "UserId": "1234",
        ///       "SecurityCode": "1111-2222-3333"               
        ///     }
        /// </remarks>
        /// <param name="UserId">UserId, who is sending the message</param>
        /// <param name="SecurityCode">SecurityCode, for the user who is sending the message</param>
        /// <returns>Returns a list of the object type UserResponse.</returns>
        ///  <response code="500">Return a server failure, i.e cannot connect to the database.</response>   
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Produces("application/json")]
        [HttpGet("UserContactList")]
        public async Task<IActionResult> GetUserContactListAsync([FromQuery] string UserId, [FromQuery] string SecurityCode)
        {
            try
            {
                //validate user inputs for userId, RecipientId and SecurityCode
                if (string.IsNullOrEmpty(UserId))
                {
                    return BadRequest("User Id is not valid");
                }
                if (string.IsNullOrEmpty(SecurityCode))
                {
                    return BadRequest("Recipient Id is not valid");
                }

                //check if user exists or if the security code belongs to that one
                var user = DB.Users.Where(u => u.SecurityCode == SecurityCode && u.UserId == UserId).SingleOrDefault();
                if (user == null)
                {
                    return BadRequest("Cannot find the user");
                }

                //get all contacts that belong to the user
                var userContacts = await DB.UserContacts.Where(uc => uc.UserId == user.UserId).Select(u => u.ContactUserId).ToListAsync();
                if (userContacts.Count() == 0)
                {
                    return BadRequest("This user doesn't have any contact yet.");
                }
                //base url
                var BaseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
              
                //get the users and the first message
                var userList = await (from users in DB.Users.Where(u => userContacts.Contains(u.UserId))
                                      from messages in DB.ChatMessages
                                      where users.UserId == messages.RecipientId
                                      select new UserResponse
                                      {
                                          UserId = user.UserId,
                                          UserName = user.UserName,
                                          Image = Path.Combine(BaseUrl, "Files", user.Image),
                                          Message = messages.Message,
                                          Date = messages.Date
                                      }

                                ).ToListAsync();
                userList = userList.GroupBy(u => u.UserId).Select(s => s.FirstOrDefault()).ToList();
                //return the list of users
                return Ok(userList);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Internal server error");
            }
        }

    }
}
