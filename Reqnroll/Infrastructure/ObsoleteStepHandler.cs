using Reqnroll.Bindings;
using Reqnroll.Configuration;
using Reqnroll.ErrorHandling;
using Reqnroll.Tracing;

namespace Reqnroll.Infrastructure
{
    public class ObsoleteStepHandler : IObsoleteStepHandler
    {
        private readonly IErrorProvider errorProvider;
        private readonly ITestTracer testTracer;
        protected readonly ReqnrollConfiguration reqnrollConfiguration;

        public ObsoleteStepHandler(IErrorProvider errorProvider, ITestTracer testTracer, ReqnrollConfiguration reqnrollConfiguration)
        {
            this.errorProvider = errorProvider;
            this.testTracer = testTracer;
            this.reqnrollConfiguration = reqnrollConfiguration;
        }

        public void Handle(BindingMatch bindingMatch)
        {
            if(bindingMatch.IsObsolete)
            {
                switch (reqnrollConfiguration.ObsoleteBehavior)
                {
                    case ObsoleteBehavior.None:
                        break;
                    case ObsoleteBehavior.Warn:
                        testTracer.TraceWarning(bindingMatch.BindingObsoletion.Message);
                        break;
                    case ObsoleteBehavior.Pending:
                        throw errorProvider.GetPendingStepDefinitionError();
                    case ObsoleteBehavior.Error:
                        throw errorProvider.GetObsoleteStepError(bindingMatch.BindingObsoletion);
                }
            }
        }
    }
}