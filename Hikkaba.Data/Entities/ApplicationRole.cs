using TPrimaryKey = System.Guid;
using Microsoft.AspNetCore.Identity;

namespace Hikkaba.Data.Entities;

public class ApplicationRole : IdentityRole<TPrimaryKey>
{
}