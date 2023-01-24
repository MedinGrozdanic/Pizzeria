using ExamContext;

namespace BackendExam.DTO
{
    public class RegistrationDTO
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public Role Roles { get; set; }

    }
}
