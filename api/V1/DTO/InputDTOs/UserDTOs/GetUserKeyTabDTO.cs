using System.ComponentModel.DataAnnotations;
namespace app_api.V1.DTO.InputDTOs.UserDTOs
{
    public class GetUserKeyTabDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public string Host { get; set; }
    }
}