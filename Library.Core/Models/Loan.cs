namespace Library.Core.Models;

public class Loan
{
    public int Id { get; set; }
    
    // Relations
    public int BookId { get; set; }
    public int UserId { get; set; }
    
    // Dates
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    
    public bool IsOverdue => DateTime.Now > DueDate && ReturnDate == null;
}