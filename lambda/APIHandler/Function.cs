using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using DatabaseLib;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace APIHandler {
	public class Function {

		/// <summary>
		/// A simple function that takes a string and does a ToUpper
		/// </summary>
		/// <param name="input"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		///
		ILambdaLogger Logger;
		public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context) {
			Logger = context.Logger;
			object resultObject = new object();
			int resultCode = 405;

			var noSignups = new APIGatewayProxyResponse() {
									Body = "{}",
									Headers = new Dictionary<string, string>() {{"access-control-allow-origin", Environment.GetEnvironmentVariable("SITE_DOMAIN")}},
									StatusCode = 418
								};
			bool enabled = bool.Parse(Environment.GetEnvironmentVariable("enabled"));

			bool freeforall = false;
		

			if(apigProxyEvent.Path.Contains("/api") && apigProxyEvent.Path.Length > 4) {
				apigProxyEvent.Path = apigProxyEvent.Path.Substring(4, apigProxyEvent.Path.Length - 4);
			}

			//Pre check the request path to save time

			switch(apigProxyEvent.Path.ToLower()) {
				case "/":
					return new StableAPIResponse {
						Body = "What are you doing here?",
						StatusCode = HttpStatusCode.OK
					};
				case "/status":
					if(apigProxyEvent.HttpMethod == "GET") {
						return new StableAPIResponse {
							Body = JsonConvert.SerializeObject(enabled),
							StatusCode = HttpStatusCode.OK
						};
					}
					return new StableAPIResponse {
						Body = "{}",
						StatusCode = HttpStatusCode.NotFound
					};

				case "/games":
				case "/games/":
				case "/participants":
				case "/participants/":
				case "/schedule":
				case "/schedule/":
				case "/scores":
				case "/scores/":
				case "/score_type":
				case "/score_type/":
				case "/print":
				case "/print/":
					break;


				default:
					return new StableAPIResponse {
						Body = "{}",
						StatusCode = HttpStatusCode.NotFound
					};
			}

			StableAPIResponse response = new StableAPIResponse {
				Body = "{}",
				StatusCode = HttpStatusCode.NotFound
			};

			string conStr = new MySqlConnectionStringBuilder() {
				Server = Environment.GetEnvironmentVariable("DB_ADDRESS"),
				Port = uint.Parse(Environment.GetEnvironmentVariable("DB_PORT")),
				UserID = Environment.GetEnvironmentVariable("DB_USER"),
				Password = Environment.GetEnvironmentVariable("DB_PASSWORD"),
				Database = Environment.GetEnvironmentVariable("DB_NAME"),
				SslMode = MySqlSslMode.Required
			}.ToString();
			using(StableContext ctx = StableContextFactory.Build(conStr)) {
				switch(apigProxyEvent.HttpMethod) {
					case "GET":
						#region GETs
						switch(apigProxyEvent.Path.ToLower()) {
							case "/":
								resultObject = "What are you doing here?";
								resultCode = 200;
								break;

							case "/games":
							case "/games/":
								response = new StableAPIResponse() {
									Body = JsonConvert.SerializeObject(ctx.Games),
									StatusCode = HttpStatusCode.OK
								};
								break;
							case "/participants":
							case "/participants/":
								response = new StableAPIResponse() {
									Body = JsonConvert.SerializeObject(ctx.Participants),
									StatusCode = HttpStatusCode.OK
								};
								break;
							case "/schedule":
							case "/schedule/":
								response = new StableAPIResponse() {
									Body = JsonConvert.SerializeObject(ctx.Schedule),
									StatusCode = HttpStatusCode.OK
								};
								break;
							case "/scores":
							case "/scores/":
								response = new StableAPIResponse() {
									Body = JsonConvert.SerializeObject(ctx.Scores),
									StatusCode = HttpStatusCode.OK
								};
								break;
							case "/score_type":
							case "/score_type/":
								response = new StableAPIResponse() {
									Body = JsonConvert.SerializeObject(ctx.ScoreType),
									StatusCode = HttpStatusCode.OK
								};
								break;
							case "/print":
							case "/print/":
								//response = HandlePrint(apigProxyEvent, ctx);
								break;
							default:
								break;
						}
						#endregion
						break;
					case "POST":
						#region POSTs
						switch(apigProxyEvent.Path.ToLower()) {

							case "/games":
							case "/games/":
								response = HandlePOST<Game>(apigProxyEvent, ctx);
								break;
							case "/participants":
							case "/participants/":
								response = HandlePOST<Participant>(apigProxyEvent, ctx);
								break;
							case "/schedule":
							case "/schedule/":
								response = HandlePOST<Schedule>(apigProxyEvent, ctx);
								break;
							case "/scores":
							case "/scores/":
								response = HandlePOST<Score>(apigProxyEvent, ctx);
								break;
							case "/score_type":
							case "/score_type/":
								response = HandlePOST<ScoreType>(apigProxyEvent, ctx);
								break;
						}
						#endregion
						break;
					case "PUT":
						#region PUTs
						switch(apigProxyEvent.Path.ToLower()) {
							case "/games":
							case "/games/":
								response = HandlePUT<Game>(apigProxyEvent, ctx, (o, tx) => {
									var g = ctx.games.FirstOrDefault(thus => thus.id == o.id);
									ctx.Entry(g).CurrentValues.SetValues(o);
									
									//var existing = ctx.Attach(o);
									//ctx.Entry(existing).State = EntityState.Modified;
									int status = ctx.SaveChanges();
									tx.Commit();
									return new StableAPIResponse() {
										Body = JsonConvert.SerializeObject((status == 1)),
										StatusCode = HttpStatusCode.OK
									};
								});
								break;
							case "/participants":
							case "/participants/":
								response = HandlePUT<Participant>(apigProxyEvent, ctx, (o, tx) => {
									var g = ctx.participants.FirstOrDefault(thus => thus.id == o.id);
									ctx.Entry(g).CurrentValues.SetValues(o);
									//var existing = ctx.Attach(o);
									//ctx.Entry(existing).State = EntityState.Modified;
									int status = ctx.SaveChanges();
									tx.Commit();
									return new StableAPIResponse() {
										Body = JsonConvert.SerializeObject((status == 1)),
										StatusCode = HttpStatusCode.OK
									};
								});
								break;
							case "/schedule":
							case "/schedule/":
								//response = HandlePUT<Schedule>(apigProxyEvent, ctx, (o, tx) => {
								//	var g = ctx.schedule.FirstOrDefault(thus => thus.g_id == o.g_id &&);
								//	ctx.Entry(g).CurrentValues.SetValues(o);
								//	//var existing = ctx.Attach(o);
								//	//ctx.Entry(existing).State = EntityState.Modified;
								//	int status = ctx.SaveChanges();
								//	tx.Commit();
								//	return new StableAPIResponse() {
								//		Body = JsonConvert.SerializeObject((status == 1)),
								//		StatusCode = HttpStatusCode.OK
								//	};
								//});
								break;
							case "/scores":
							case "/scores/":
								response = HandlePUT<Score>(apigProxyEvent, ctx, (o, tx) => {
									var g = ctx.scores.FirstOrDefault(thus => thus.g_id == o.g_id && thus.p_id == o.p_id);
									ctx.Entry(g).CurrentValues.SetValues(o);
									//var existing = ctx.Attach(o);
									//ctx.Entry(existing).State = EntityState.Modified;
									int status = ctx.SaveChanges();
									tx.Commit();
									return new StableAPIResponse() {
										Body = JsonConvert.SerializeObject((status == 1)),
										StatusCode = HttpStatusCode.OK
									};
								});
								break;
							case "/score_type":
							case "/score_type/":
								response = HandlePUT<ScoreType>(apigProxyEvent, ctx, (o, tx) => {
									var g = ctx.score_type.FirstOrDefault(thus => thus.id == o.id);
									ctx.Entry(g).CurrentValues.SetValues(o);
									//var existing = ctx.Attach(o);
									//ctx.Entry(existing).State = EntityState.Modified;
									int status = ctx.SaveChanges();
									tx.Commit();
									return new StableAPIResponse() {
										Body = JsonConvert.SerializeObject((status == 1)),
										StatusCode = HttpStatusCode.OK
									};
								});
								break;
						}
						break;
					#endregion
					case "DELETE":
						#region DELETEs
						switch(apigProxyEvent.Path.ToLower()) {
							case "/games":
							case "/games/":
								response = HandleDELETE<Game>(apigProxyEvent, ctx);
								break;
							case "/participants":
							case "/participants/":
								response = HandleDELETE<Participant>(apigProxyEvent, ctx);
								break;
							case "/schedule":
							case "/schedule/":
								response = HandleDELETE<Schedule>(apigProxyEvent, ctx);
								break;
							case "/scores":
							case "/scores/":
								response = HandleDELETE<Score>(apigProxyEvent, ctx);
								break;
							case "/score_type":
							case "/score_type/":
								response = HandleDELETE<ScoreType>(apigProxyEvent, ctx);
								break;
							default:
								break;
						}
						#endregion
						break;
					default:
						break;
				}
			}
			//Logger.LogLine($"RESPONSE CODE: {((HttpStatusCode)response.StatusCode).ToString()}{Environment.NewLine}{response.Body}");

			return response;
		}

		//You gotta love generic typing!! :D

		private StableAPIResponse HandlePOST<E> (APIGatewayProxyRequest request, StableContext ctx) where E : class {
			try {
				string adminCode = Environment.GetEnvironmentVariable("admin_code");
				if(adminCode == null || adminCode == "")
					throw new InvalidOperationException("admin_code not set on server");

				if(!request.Headers.ContainsKey("admin_code"))
					throw new ArgumentException("admin_code is missing");

				if(request.Headers["admin_code"] != adminCode)
					throw new UnauthorizedAccessException("Invalid admin_code");

				E obj = JsonConvert.DeserializeObject<E>(request.Body);

				using(var tx = ctx.Database.BeginTransaction()) {
					try {
						ctx.Add(obj);
						int status = ctx.SaveChanges();
						tx.Commit();
						return new StableAPIResponse() {
							Body = JsonConvert.SerializeObject((status == 1)),
							StatusCode = HttpStatusCode.OK
						};
					} catch(Exception e) {
						tx.Rollback();
						Logger.LogLine(e.ToString());
						return new StableAPIResponse() {
							Body = JsonConvert.SerializeObject(new Result(e)),
							StatusCode = HttpStatusCode.InternalServerError
						};
					}
				}

			} catch(Exception e) {
				Logger.LogLine(e.ToString());
				return StableAPIResponse.BadRequest(e);
			}
		}
		private StableAPIResponse HandlePUT<E>(APIGatewayProxyRequest request, StableContext ctx, Func<E, IDbContextTransaction, StableAPIResponse> func) where E : class {
			try {
				string adminCode = Environment.GetEnvironmentVariable("admin_code");
				if(adminCode == null || adminCode == "")
					throw new InvalidOperationException("admin_code not set on server");

				if(!request.Headers.ContainsKey("admin_code"))
					throw new ArgumentException("admin_code is missing");

				if(request.Headers["admin_code"] != adminCode)
					throw new UnauthorizedAccessException("Invalid admin_code");

				E obj = JsonConvert.DeserializeObject<E>(request.Body);

				using(var tx = ctx.Database.BeginTransaction()) {
					try {
						return func(obj, tx);
						var existing = ctx.Attach(obj);
						ctx.Entry(existing).State = EntityState.Modified;
						int status = ctx.SaveChanges();
						tx.Commit();
						return new StableAPIResponse() {
							Body = JsonConvert.SerializeObject((status == 1)),
							StatusCode = HttpStatusCode.OK
						};
					} catch(Exception e) {
						tx.Rollback();
						Logger.LogLine(e.ToString());
						return new StableAPIResponse() {
							Body = JsonConvert.SerializeObject(new Result(e)),
							StatusCode = HttpStatusCode.InternalServerError
						};
					}
				}

			} catch(Exception e) {
				Logger.LogLine(e.ToString());
				return StableAPIResponse.BadRequest(e);
			}
		}
		private StableAPIResponse HandleDELETE<E>(APIGatewayProxyRequest request, StableContext ctx) where E : class {
			try {
				string adminCode = Environment.GetEnvironmentVariable("admin_code");
				if(adminCode == null || adminCode == "")
					throw new InvalidOperationException("admin_code not set on server");

				if(!request.Headers.ContainsKey("admin_code"))
					throw new ArgumentException("admin_code is missing");

				if(request.Headers["admin_code"] != adminCode)
					throw new UnauthorizedAccessException("Invalid admin_code");

				E obj = JsonConvert.DeserializeObject<E>(request.Body);
				/*
				 * Gotta wrap DB ops in a transaction
				 * otherwise, if they die in a try catch
				 * it could leave an uncommitted tx on the db
				 * causing problems with future requests
				using(var tx = ctx.Database.BeginTransaction()) {
					
				}
				*/
				using(var tx = ctx.Database.BeginTransaction()) {
					try {
						//Logger.LogLine(obj.GetType().ToString());
						ctx.Remove(obj);
						//ctx.Attach(obj);
						//ctx.Remove(obj);
						//ctx.dates.Remove(ctx.dates.Single(thus => thus.date == date));
						int status = ctx.SaveChanges();
						tx.Commit();
						return new StableAPIResponse() {
								Body = JsonConvert.SerializeObject((status == 1)),
								StatusCode = HttpStatusCode.OK
							};
					} catch(Exception e) {
						tx.Rollback();
						Logger.LogLine(e.ToString());
						return new StableAPIResponse() {
							Body = JsonConvert.SerializeObject(new Result(e)),
							StatusCode = HttpStatusCode.InternalServerError
						};
					}
				}

			} catch(Exception e) {
				Logger.LogLine(e.ToString());
				return StableAPIResponse.BadRequest(e);
			}
		}
		
		/*
		private StableAPIResponse HandlePrint(APIGatewayProxyRequest request, StableContext ctx) {

			uint pres_id;
			try {
				try {
				pres_id = uint.Parse(request.QueryStringParameters["presentation_id"]);
				} catch(Exception e) {
					return StableAPIResponse.BadRequest(e);
				}
				var presentation = ctx.presentations.First(thus => thus.presentation_id == pres_id);
				var location = ctx.locations.First(thus => thus.location_id == presentation.location_id);
				var blocks = ctx.Blocks;
				var grades = ctx.Grades;
				var houses = ctx.Houses;

				var schedules = ctx.schedule.Where(thus => thus.presentation_id == pres_id).ToList();
				var viewers = new Dictionary<Schedule, List<Viewer>>();

				foreach(Schedule s in schedules) {
					viewers.Add(s, new List<Viewer>());
				}

				var temp = ctx.registrations.Where(thus => thus.presentation_id == pres_id).ToList();

				var viewers_in_pres = from thus in temp select thus.viewer_id;
				
				var viewers_with_data = ctx.viewers.Where(thus => viewers_in_pres.Contains(thus.viewer_id)).ToList();

				foreach(Registration r in temp) {
					viewers[r.Schedule].Add(viewers_with_data.First(thus => thus.viewer_id == r.viewer_id));
				}


				PrintOutput print = new PrintOutput() {
					presentationData = presentation,
					locationData = location,
					blocks = blocks,
					schedule = schedules,
					grades = grades,
					houses = houses,
					viewers = viewers

				};
				var res = new StableAPIResponse() {
					Body = print.ToString(),
					StatusCode = HttpStatusCode.OK,
					
				};
				res.Headers.Add("Content-Type", "text/html; charset=utf-8");
				return res;
			} catch(Exception e) {
				throw;
			}
		}
		*/
	
	}
	public class StableAPIResponse : APIGatewayProxyResponse {
		public StableAPIResponse() {
			Headers = new Dictionary<string, string>() {
				{ "access-control-allow-origin", Environment.GetEnvironmentVariable("SITE_DOMAIN") }
			};
		}
		new public HttpStatusCode StatusCode {
			get {
				return (HttpStatusCode)base.StatusCode;
			}
			set {
				base.StatusCode = (int)value;
			}
		}
		public static StableAPIResponse OK {
			get {
				return new StableAPIResponse() {
					Body = "{}",
					StatusCode = HttpStatusCode.OK
				};
			}
		}
		public static StableAPIResponse NotImplemented {
			get {
				return new StableAPIResponse() {
					Body = "{}",
					StatusCode = HttpStatusCode.NotImplemented
				};
			}
		}
		public static StableAPIResponse Unauthorized {
			get {
				return new StableAPIResponse() {
					Body = "{}",
					StatusCode = HttpStatusCode.Unauthorized
				};
			}
		}
		public static StableAPIResponse BadRequest(Exception e) {
			return new StableAPIResponse() {
				StatusCode = HttpStatusCode.BadRequest,
				Body = JsonConvert.SerializeObject(new Result(e))
			};
		}
		public static StableAPIResponse InternalServerError(Exception e) {
			return new StableAPIResponse() {
				StatusCode = HttpStatusCode.InternalServerError,
				Body = JsonConvert.SerializeObject(new Result(e))
			};
		}
	}
}
