﻿namespace FinalProject.Dto
{
    public class UserLoginPrinciple
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role {  get; set; }
        public bool IsActive { get; set; }
        public string TypeAuthentication {  get; set; }
    }
}
