using System.Text.Json;
using Infrastructure.Db.App.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Models.User;

public class UpdateUserRequest
{
    /// <summary>
    /// Пользователь подтвердил ознакомление с политикой и документами.
    /// </summary>
    public bool? ConfirmedPolicyAndDocuments { get; set; }
}
