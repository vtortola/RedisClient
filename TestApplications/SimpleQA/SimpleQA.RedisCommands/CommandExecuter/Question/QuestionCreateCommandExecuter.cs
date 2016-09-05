using SimpleQA.Commands;
using SimpleQA.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA.RedisCommands
{
    public sealed class QuestionCreateCommandExecuter : ICommandExecuter<QuestionCreateCommand, QuestionCreateCommandResult>
    {
       readonly IRedisChannel _channel;

       public QuestionCreateCommandExecuter(IRedisChannel channel)
       {
           _channel = channel;
       }

       public async Task<QuestionCreateCommandResult> ExecuteAsync(QuestionCreateCommand command, IPrincipal user, CancellationToken cancel)
       {
           var store = Keys.QuestionIdStore();

           var result = await _channel.ExecuteAsync(@"INCR @store", new { store }).ConfigureAwait(false);
           var id = result[0].GetInteger().ToString();

           var slug = RemoveSymbols(command.Title);
           slug = RemoveDiacritics(slug);
           if (String.IsNullOrWhiteSpace(slug))
               throw new SimpleQAException("Unable to generate a slug");

           var initialScore = command.CreationDate.Ticks;

           var data = GetQuestionData(command, user, id, slug, initialScore);

           result = await _channel.ExecuteAsync(@"
                        MULTI
                        SaveQuestionData @questionKey @data @userVotes
                        IndexQuestion @questionKey @questionByTime @questionByScore @initialScore @questionCounter
                        NotifyTags @tagKeys
                        SaveQuestionTags @qtagKey @autocompleteTags @Tags 
                        RegisterQuestionTagsByScore @questionKey @tagScoreKeys @scoreIncr
                        RegisterQuestionTagsByDate @questionKey @tagDateKeys @initialScore
                        IndexQuestionTags @Tags @tagsByScore @scoreIncr @tagsByDate @initialScore @tagCounting
                        EXEC",
                       new
                       {
                           questionKey = Keys.QuestionKey(id),
                           data,
                           questionByTime = Keys.QuestionsByDate(),
                           questionByScore = Keys.QuestionsByScore(),
                           initialScore,
                           questionCounter = Keys.QuestionCounter(),
                           tagScoreKeys = command.Tags.Select(t => Keys.TagKeyByScore(t)),
                           tagDateKeys = command.Tags.Select(t => Keys.TagKeyByDate(t)),
                           command.Tags,
                           tagsByScore = Keys.TagsByScore(),
                           scoreIncr = Constant.VoteScore,
                           tagsByDate = Keys.TagsByDate(),
                           qtagKey = Keys.QuestionTagsKey(id),
                           tagCounting = Keys.TagCounting(),
                           userVotes = Keys.UserVotesKey(user.Identity.Name),
                           autocompleteTags = Keys.AutoCompleteTags(),
                           tagKeys = command.Tags.Select(t=>Keys.TagNotification(t))
                       })
                       .ConfigureAwait(false);

           result.ThrowErrorIfAny();
           result.Last().AssertOK();

           return new QuestionCreateCommandResult(id, slug);
       }

       static IEnumerable<String> GetQuestionData(QuestionCreateCommand command, IPrincipal user, String id, String slug, Int64 initialScore)
       {
           var data = Parameter.SequenceProperties(new
           {
               Id = id,
               Title = command.Title,
               Content = command.Content,
               HtmlContent = command.HtmlContent,
               CreatedOn = command.CreationDate,
               User = user.Identity.Name,
               Slug = slug,
               Score = initialScore,
               UpVotes = 0,
               DownVotes = 0,
               AnswerCount = 0,
               ViewCount = 0,
               Status = QuestionStatus.Open,
               ContentExcerpt = GetExcerpt(command.Content, 50)
           });
           return data;
       }

       static String RemoveSymbols(String value)
       {
           var buffer = new Char[value.Length];
           var count = 0;
           var prevWasSymbol = false;
           for (int i = 0; i < value.Length; i++)
           {
               var c = value[i];
               if (!Char.IsLetterOrDigit(c))
               {
                   if (!prevWasSymbol)
                       buffer[count++] = '-';
                   prevWasSymbol = true;
               }
               else
               {
                   buffer[count++] = c;
                   prevWasSymbol = false;
               }
           }
           while (buffer[count - 1] == '-')
               count--;
           return new String(buffer, 0, count);
       }

       static String RemoveDiacritics(String text)
       {
           var normalizedString = text.Normalize(NormalizationForm.FormD);
           var stringBuilder = new StringBuilder();

           foreach (var c in normalizedString)
           {
               var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
               if (unicodeCategory != UnicodeCategory.NonSpacingMark)
               {
                   stringBuilder.Append(c);
               }
           }

           return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
       }

       static String GetExcerpt(String markdown, Int32 length)
       {
           var array = new Char[length];
           var index = 0;
           for (int i = 0; i < markdown.Length; i++)
           {
               var c = markdown[i];

               if ((c == '\r' || c == '\n' || c == '\t' || c == ' ') && array[index] != ' ')
                   array[index++] = ' ';

               if (Char.IsLetterOrDigit(c))
               {
                   array[index++] = c;
               }

               if (index == length)
                   break;
           }
           return new String(array, 0, index);
       }
    }
}
