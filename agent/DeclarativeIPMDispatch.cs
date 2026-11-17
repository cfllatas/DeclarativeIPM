#nullable disable
using pm4h.svelta;
using pm4h.svelta.chains;
using PMAppSVELTA.circuits;
using PMAppSVELTA.declare;
using PMAppSVELTA.milestone;
using PMAppSVELTA.regions;
using PMAppSVELTA.src.chat;
using System.Threading.Tasks;

namespace PMAppSVELTA.DeclarativeIPM
{
    /// <summary>
    /// Applies the operation <see cref="DeclarativeIPMAgent"/> left in query.json_result, according to "kind", with SVELTA's Apply* chains (which read the same
    /// payload: expression / circuits / patterns / nodes). With no operation (error or clarification) it does nothing: query.result already holds the message.
    /// </summary>
    public class DeclarativeIPMDispatch : Chain
    {
        public override async Task<ChainValue> RunStepAsync(ChainValue ctx)
        {
            var query = ctx.Wrap<ILLMQuery>();
            var json = query.json_result;
            if (json == null) return ctx;

            string kind = json["kind"]?.ToString();
            string name = json["name"]?.ToString();
            string what;
            Chain apply;
            switch (kind)
            {
                case "filter": apply = new ApplyDeclare2Perspective(); what = "New view"; break;
                case "circuits": apply = new ApplyCircuits(); what = "Circuits defined"; break;
                case "regions": apply = new ApplyRegions(); what = "Regions defined"; break;
                case "milestones": apply = new ApplyMilestone(); what = "Milestones defined"; break;
                default: return ctx;
            }

            ctx = await apply.RunStepAsync(ctx);

            // The Apply* chains leave "Done!" / "Hecho!": replace it with a message saying what was done (no LLM call).
            if (query.result is "Done!" or "Hecho!")
                query.result = string.IsNullOrWhiteSpace(name) ? what + "." : $"{what}: {name}.";
            return ctx;
        }
    }
}
