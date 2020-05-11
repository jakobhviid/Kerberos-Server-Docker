using System;
using Microsoft.AspNetCore.Mvc;
using app_api.V1.Repos.UserRepo;
using app_api.V1.DTO.InputDTOs.UserDTOs;
using Microsoft.AspNetCore.Http;
using app_api.V1.Contracts;
using app_api.Data.Models;
using app_api.V1.DTO.OutputDTOs;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace app_api.V1.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepo _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserController(IUserRepo repo, IMapper mapper, IConfiguration configuration)
        {
            _repo = repo;
            _mapper = mapper;
            _configuration = configuration;
        }

        [HttpPost(ApiRoutes.UserRoutes.Register)]
        public async Task<ActionResult> RegisterUser(NewUserDTO input)
        {
            // Check that admin password is correct to the one supplied in environment files
            if (!ValidAdmin(input.AdminPassword))
                return StatusCode(StatusCodes.Status403Forbidden, new GenericReturnMessageDTO { StatusCode = 403, Message = ErrorMessages.AdminPasswordInCorrect });

            // Check that the username is not already in use in another keytab
            if (await _repo.UserExists(input.NewUserUsername))
                return BadRequest(new GenericReturnMessageDTO { StatusCode = 400, Message = ErrorMessages.UserAlreadyExists });

            // Create users principal and keytab
            $"create-user.sh {input.NewUserUsername}".Bash();

            var newUserKeyTabFilePath = $"/keytabs/{input.NewUserUsername}.user.keytab";

            var user = await _repo.CreateNewUser(_mapper.Map<User>(input), input.NewUserPassword, newUserKeyTabFilePath);

            return StatusCode(StatusCodes.Status201Created, new GenericReturnMessageDTO
            {
                StatusCode = 201,
                Message = SuccessMessages.UserCreated
            });
        }

        [HttpGet(ApiRoutes.UserRoutes.GetKeyTab)]
        public async Task<ActionResult> GetKeyTab(GetUserKeyTabDTO input)
        {
            if (input.Host != null) {
                input.Username = input.Username + "/" + input.Host;
            }
            Console.WriteLine(input.Username);
            var keyTab = await _repo.GetUserKeyTab(input.Username, input.Password);
            if (keyTab == null)
                return BadRequest(new GenericReturnMessageDTO { StatusCode = 400, Message = ErrorMessages.UserDoesNotExist });

            return File(keyTab, "application/octet-stream");
        }

        [HttpPost(ApiRoutes.UserRoutes.RegisterService)]
        public async Task<ActionResult> RegisterService(NewServiceDTO input)
        {
            // Check that admin password is correct to the one supplied in environment files
            if (!ValidAdmin(input.AdminPassword))
                return StatusCode(StatusCodes.Status403Forbidden, new GenericReturnMessageDTO { StatusCode = 403, Message = ErrorMessages.AdminPasswordInCorrect });

            var userNameWithHost = input.NewServiceName + "/" + input.NewServiceHost;
            // Check that the username is not already in use in another keytab
            if (await _repo.UserExists(userNameWithHost))
                return BadRequest(new GenericReturnMessageDTO { StatusCode = 400, Message = ErrorMessages.ServiceAlreadyExists });

            // Create service principal and keytab
            $"create-service.sh {input.NewServiceName} {input.NewServiceHost}".Bash();

            var newUserKeyTabFilePath = $"/keytabs/{input.NewServiceName}.service.keytab";

            var user = await _repo.CreateNewUser(_mapper.Map<User>(input), input.NewServicePassword, newUserKeyTabFilePath);

            return StatusCode(StatusCodes.Status201Created, new GenericReturnMessageDTO
            {
                StatusCode = 201,
                Message = SuccessMessages.ServiceCreated
            });
        }

        private bool ValidAdmin(string adminPassword)
        {
            return adminPassword.Equals(_configuration["KERBEROS_ADMIN_PW"]);
        }


    }
}