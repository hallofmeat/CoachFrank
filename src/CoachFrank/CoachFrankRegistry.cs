using Autofac;
using CoachFrank.Data;

namespace CoachFrank;

public class CoachFrankRegistry : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        //EfCore
        builder
            .RegisterType<BotContext>()
            .InstancePerLifetimeScope();
    }
}