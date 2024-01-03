using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGPT.Data.Model
{
    public class UserToken
    {
        [Key]
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public int PromptTokens { get; set; } = 0;
        public int CompletionTokens { get; set; } = 0;
        public int Credit { get; set; } = 0;
    }
}
