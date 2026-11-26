using System.ComponentModel.DataAnnotations;
using MovieVault.Api.Entities;

namespace MovieVault.Api.DTOs;

// Bruges når en anmeldelse oprettes (POST).
// Forfatter og medie bestemmes af serveren ud fra den indloggede bruger.
public class CreateReviewDto
{
    [Range(1, int.MaxValue, ErrorMessage = "MovieId must be a valid id.")]
    public int MovieId { get; set; }

    [Required(ErrorMessage = "Headline is required.")]
    [StringLength(150, ErrorMessage = "Headline can be at most 150 characters.")]
    public string Headline { get; set; } = string.Empty;

    [Required(ErrorMessage = "Review text is required.")]
    [StringLength(4000, ErrorMessage = "Review text can be at most 4000 characters.")]
    public string Body { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    // Kun for Admin: hvilken anmelder anmeldelsen skrives for.
    // En Reviewer må udelade feltet (eller sende sit eget id) - et andet id giver 403.
    [Range(1, int.MaxValue, ErrorMessage = "ReviewerId must be a valid id.")]
    public int? ReviewerId { get; set; }
}

// Bruges når en anmeldelse ændres (PUT). Film, forfatter, medie og dato kan ikke ændres.
public class UpdateReviewDto
{
    [Required(ErrorMessage = "Headline is required.")]
    [StringLength(150, ErrorMessage = "Headline can be at most 150 characters.")]
    public string Headline { get; set; } = string.Empty;

    [Required(ErrorMessage = "Review text is required.")]
    [StringLength(4000, ErrorMessage = "Review text can be at most 4000 characters.")]
    public string Body { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }
}

// Det API'et returnerer for en anmeldelse
public class ReviewDto
{
    public int ReviewId { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int Rating { get; set; }
    public DateTime PublishedAt { get; set; }

    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;

    public int ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;

    public int MediaOutletId { get; set; }
    public string MediaOutletName { get; set; } = string.Empty;
    public MediaOutletType MediaOutletType { get; set; }
}
