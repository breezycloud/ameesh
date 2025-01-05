using Shared.Enums;

namespace Shared.Models.Expenses;

public record ExpenseData(Guid typeId, string? expense, string? description, string? reference, string? user, decimal amount, PaymentMode mode, DateTime date);