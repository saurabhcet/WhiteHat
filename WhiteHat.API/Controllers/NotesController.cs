using WhiteHat.API.DataContext;
using WhiteHat.API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Runtime.CompilerServices;
using WhiteHat.API.Email;

namespace MeetiWhiteHat.API.Controllers
{
    [ApiController]
    [Route("/[controller]/[action]")]
    public class NotesController : ControllerBase
    {
        private readonly ILogger<NotesController> _logger;

        private readonly INoteRepository _noteRepository;

        private readonly IEmailSender _emailSender;

        public NotesController(INoteRepository noteRepository, IEmailSender emailSender, ILogger<NotesController> logger)
        {
            _logger = logger;
            _noteRepository = noteRepository;
            _emailSender = emailSender;
        }

        [HttpPost]
        [ActionName("AddNote")]
        public ActionResult AddNote(string sessionId, string note)
        {
            try
            {
                _logger.LogInformation($"Adding note for session id:{sessionId} Note = {note}");
                return Ok(_noteRepository.AddNote(sessionId, note));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, null);
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete]
        [ActionName("DeleteNote")]
        public ActionResult<bool> DeleteNote(string sessionId, int index)
        {
            try
            {
                _logger.LogInformation($"Deleting note for session id:{sessionId} Index = {index}");
                return Ok(_noteRepository.DeleteNote(sessionId, index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, null);
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [ActionName("GetMoM")]
        public ActionResult<Minute> GetMoM(string sessionId)
        {
            try
            {
                _logger.LogInformation($"Getting mom details for session id:{sessionId}");
                return Ok(_noteRepository.GetMoM(sessionId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, null);
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }          
        }

        [HttpPost]
        [ActionName("EmailMOM")]
        public ActionResult<bool> EmailMoM(string sessionId)
        {
            try
            {
                _logger.LogInformation($"Getting mom details for session id:{sessionId}");
                var mom = _noteRepository.GetMoM(sessionId);
                mom.Subject = $"WhiteHatBot-MOM-{sessionId}-{DateTime.Now}";
                _logger.LogInformation($"sending email for session id:{sessionId}");
                var result = _emailSender.SendMail(mom);
                _logger.LogInformation($"sent email for session id:{sessionId}. Result = {result}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, null);
                return Problem(detail:ex.Message, statusCode: StatusCodes.Status500InternalServerError); 
            }
        }

        [HttpPost]
        [ActionName("AddParticipants")]
        public ActionResult<bool> AddParticipants(string sessionId, string participants)
        {
            try
            {
                if (string.IsNullOrEmpty(participants))
                    throw new ArgumentNullException(nameof(participants));

                var mom = _noteRepository.GetMoM(sessionId);
                if (mom == null)
                    throw new Exception($"Unable to find minutes for session id{sessionId}");

                _logger.LogInformation($"Adding participants for session id:{sessionId} participants = {participants}");

                mom.ParticipantsColSeparated = participants;
                _noteRepository.UpdateMom(mom);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, null);
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        //[HttpGet]
        //[ActionName("GetNotes")]
        //public ActionResult<IEnumerable<Minute>> GetNotes()
        //{
        //    return Ok(_noteRepository.GetNotes());
        //}

        [HttpPost]
        [ActionName("SendTestMail")]
        public void SendTestMail(string subject)
        {
            _emailSender.SendTestMail(subject);

        }



    }
}