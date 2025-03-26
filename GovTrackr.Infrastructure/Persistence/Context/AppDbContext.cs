using Microsoft.EntityFrameworkCore;

namespace GovTrackr.Infrastructure.Persistence.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options);