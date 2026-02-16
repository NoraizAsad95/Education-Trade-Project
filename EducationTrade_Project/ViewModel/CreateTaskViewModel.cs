using EducationTrade.Core.Entities;
using System.ComponentModel.DataAnnotations;


namespace EducationTrade.Web.ViewModels
{
    public class CreateTaskViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Coin reward is required")]
        [Range(1, 1000, ErrorMessage = "Coin reward must be between 1 and 1000")]
        public int CoinReward { get; set; }

        // Display info (not submitted)
        public int UserCoinBalance { get; set; }
        public string? UserName { get; set; }
    }
}