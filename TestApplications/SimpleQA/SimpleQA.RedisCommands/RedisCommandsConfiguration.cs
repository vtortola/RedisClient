using SimpleInjector;
using SimpleInjector.Diagnostics;
using SimpleQA.Commands;
using SimpleQA.Models;
using SimpleQA.RedisCommands;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace SimpleQA
{
    public static class RedisCommandsConfiguration
    {
        public static void Configure(Container container, IPEndPoint endpoint, IRedisClientLog log)
        {
            var options = new RedisClientOptions() 
            { 
                //PingTimeout = TimeSpan.FromSeconds(1),
                //PreventPingIfActive = false,
                Logger = log
            };

            options.Procedures.LoadFromAssembly(Assembly.GetExecutingAssembly());

            var client = new RedisClient(endpoint, options);

            var connect = client.ConnectAsync(CancellationToken.None);

            container.RegisterSingleton<RedisClient>(client);
            container.Register<IRedisChannel>(() => client.CreateChannel(), Lifestyle.Scoped);

            container.Register<IMessaging>(() => new RedisClientMessaging(client.CreateChannel()));
            
            container.Register<ICommandExecuter<VisitQuestionCommand, VisitQuestionCommandResult>, VisitQuestionCommandExecuter>();

            container.Register<ICommandExecuter<AnswerEditCommand, AnswerEditCommandResult>, AnswerEditCommandExecuter>();
            container.Register<ICommandExecuter<AnswerDeleteCommand, AnswerDeleteCommandResult>, AnswerDeleteCommandExecuter>();
            container.Register<ICommandExecuter<AnswerVoteCommand, AnswerVoteCommandResult>, AnswerVoteCommandExecuter>();
            container.Register<ICommandExecuter<AnswerCreateCommand, AnswerCreateCommandResult>, AnswerCreateCommandExecuter>();

            container.Register<ICommandExecuter<QuestionDeleteCommand, QuestionDeleteCommandResult>, QuestionDeleteCommandExecutor>();
            container.Register<ICommandExecuter<QuestionCloseCommand, QuestionCloseCommandResult>, QuestionCloseCommandExecuter>();
            container.Register<ICommandExecuter<QuestionEditCommand, QuestionEditCommandResult>, QuestionEditCommandExecuter>();
            container.Register<ICommandExecuter<QuestionVoteCommand, QuestionVoteCommandResult>, QuestionVoteCommandExecuter>();
            container.Register<ICommandExecuter<QuestionCreateCommand, QuestionCreateCommandResult>, QuestionCreateCommandExecuter>();

            container.Register<ICommandExecuter<EndSessionCommand, EndSessionCommandResult>, EndSessionCommandExecuter>();
            container.Register<ICommandExecuter<ValidateSessionCommand, ValidateSessionCommandResult>, ValidateSessionCommandExecuter>();
            container.Register<ICommandExecuter<AuthenticateCommand, AuthenticateCommandResult>, AuthenticateCommandExecuter>();

            container.Register<IModelBuilder<UserModelRequest, UserModel>, UserModelBuilder>();
            container.Register<IModelBuilder<UserInboxRequest, UserInboxModel>, UserInboxModelBuilder>();

            container.Register<IModelBuilder<AnswerRequest, AnswerViewModel>, AnswerRequestBuilder>();
            container.Register<IModelBuilder<AnswerEditFormRequest, AnswerEditFormViewModel>, AnswerEditFormRequestBuilder>();
            container.Register<IModelBuilder<AnswerDeleteFormRequest, AnswerDeleteFormViewModel>, AnswerDeleteFormRequestBuilder>();

            container.Register<IModelBuilder<HomeByTagRequest, HomeByTagViewModel>, HomeByTagRequestBuilder>();
            container.Register<IModelBuilder<HomeRequest, HomeViewModel>, HomeRequestBuilder>();

            container.Register<IModelBuilder<QuestionDeleteFormRequest, QuestionDeleteFormViewModel>, QuestionDeleteFormRequestBuilder>();
            container.Register<IModelBuilder<QuestionCloseFormRequest, QuestionCloseFormViewModel>, QuestionCloseFormRequestBuilder>();
            container.Register<IModelBuilder<QuestionEditFormRequest, QuestionEditFormViewModel>, QuestionEditFormRequestBuilder>();
            container.Register<IModelBuilder<QuestionRequest, QuestionViewModel>, QuestionRequestBuilder>();

            container.Register<IModelBuilder<TagSuggestionRequest, TagSuggestionsModel>, TagSuggestionRequestBuilder>();
            container.Register<IModelBuilder<PopularTagsRequest, PopularTagsViewModel>, PopularTagsRequestBuilder>();

            Registration registration = container.GetRegistration(typeof(IMessaging)).Registration;
            registration.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "Manually disposed");

            connect.Wait();
        }
    }
}
