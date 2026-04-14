namespace Library.Core.Models;

public class Loan
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public int UserId { get; set; }

    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    // Display fields (from JOINs)
    public string BookTitle { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    public bool IsOverdue => DateTime.Now > DueDate && ReturnDate == null;
    public bool IsActive => ReturnDate == null;
}
