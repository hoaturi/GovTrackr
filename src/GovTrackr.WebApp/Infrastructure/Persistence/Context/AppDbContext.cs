using Microsoft.EntityFrameworkCore;

namespace GovTrackr.Application.Infrastructure.Persistence.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options);