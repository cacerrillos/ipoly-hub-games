using Microsoft.EntityFrameworkCore;
using MySQL.Data.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DatabaseLib {
	public class StableContextFactory {
		public static StableContext Build(string conStr) {
			var optionsBuilder = new DbContextOptionsBuilder<StableContext>();
			//optionsBuilder.
			optionsBuilder.UseMySQL(conStr);

			return new StableContext(optionsBuilder.Options);
		}
	}
	public class StableContext : DbContext {
		public StableContext(DbContextOptions<StableContext> options) : base(options) {
			//Model.
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<Schedule>()
				.HasKey(c => new {
					c.p_id,
					c.block,
					c.g_id
				});
			modelBuilder.Entity<Score>()
				.HasKey(c => new {
					c.g_id,
					c.p_id
				});

			modelBuilder.Entity<Game>()
				.HasIndex(c => c.score_type);

			modelBuilder.Entity<Score>()
					.ToTable("scores");
		}

		public DbSet<Game> games {
			get;
			set;
		}
		public Dictionary<uint, Game> Games {
			get {
				var result = new Dictionary<uint, Game>();

				foreach(var g in games.ToList()) {
					result.Add(g.id, g);
				}

				return result;
			}
		}
		public DbSet<Participant> participants {
			get;
			set;
		}
		public Dictionary<uint, Participant> Participants {
			get {
				var result = new Dictionary<uint, Participant>();

				foreach(var p in participants.ToList()) {
					result.Add(p.id, p);
				}

				return result;
			}
		}
		public DbSet<Schedule> schedule {
			get;
			set;
		}
		public Dictionary<uint, List<Schedule>> Schedule {
			get {
				var result = new Dictionary<uint, List<Schedule>>();
				foreach(var s in schedule.OrderBy(thus => thus.p_id)) {
					if(!result.ContainsKey(s.block))
						result.Add(s.block, new List<DatabaseLib.Schedule>());

					result[s.block].Add(s);
				}
				return result;
			}
		}
		public DbSet<Score> scores {
			get;
			set;
		}
		public Dictionary<uint, List<Score>> Scores {
			get {
				var result = new Dictionary<uint, List<Score>>();

				foreach(var s in scores) {
					if(!result.ContainsKey(s.g_id))
						result.Add(s.g_id, new List<Score>());


					result[s.g_id].Add(s);
				}

				return result;
			}
		}
		public DbSet<ScoreType> score_type {
			get;
			set;
		}
		public Dictionary<uint, ScoreType> ScoreType {
			get {
				var result = new Dictionary<uint, ScoreType>();

				foreach(var s in score_type.ToList()) {
					result.Add(s.id, s);
				}

				return result;
			}
		}

	}
	
}
