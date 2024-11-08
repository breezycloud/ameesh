namespace Shared.Models.Expenses;

public record ExpenseEntryDto(Guid StoreId, Guid UserId, Guid TypeId, string ReferenceNo, DateTime  CreatedDate);