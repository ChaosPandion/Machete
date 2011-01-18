using Machete.Core;

namespace Machete.Runtime.NativeObjects
{
    public class NBoundFunction : NFunction
    {
        public IObject TargetFunction { get; set; }
        public IDynamic BoundThis { get; set; }
        public IArgs BoundArguments { get; set; }

        public NBoundFunction(IEnvironment enviroment)
            : base(enviroment)
        {

        }

        protected override IDynamic Call(IEnvironment environment, IArgs args)
        {
            var func = TargetFunction as NFunction;
            return func.Call(environment, BoundThis, environment.ConcatArgs(BoundArguments, args));
        }

        public override IObject Construct(IEnvironment environment, IArgs args)
        {
            var func = TargetFunction as NFunction;
            if (func == null)
            {
                throw Environment.CreateTypeError("");
            }
            return func.Construct(environment, environment.ConcatArgs(BoundArguments, args));
        }

        public override bool HasInstance(IDynamic value)
        {
            var func = TargetFunction as IHasInstance;
            if (func == null)
            {
                throw Environment.CreateTypeError("");
            }
            return func.HasInstance(value);
        }
    }
}