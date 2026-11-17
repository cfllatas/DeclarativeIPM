#nullable disable
using pm4h.filter.fineanalysis;
using pm4h.runner;
using pm4h.windows.ui.blocks;
using pm4h.windows.ui.fragments.appmanager;
using pm4hlab.ext.tpaengine.declaregrammar;
using pm4hlab.ext.tpaengine.declaregrammar.runnerblocks;
using System.Linq;

namespace PMAppSVELTA.DeclarativeIPM
{
    /// <summary>
    /// What the interactive runner currently holds for the three operations that leave state (only one definition of each can exist).
    /// The filter leaves no state: it opens a separate view.
    /// </summary>
    public class DeclarativeIPMState
    {
        public string Circuits { get; set; }
        public string Regions { get; set; }
        public string[] Milestones { get; set; }

        public static DeclarativeIPMState Read(ExperimentArguments args)
        {
            var st = new DeclarativeIPMState();
            if (args?.Runner is UIRunner rnr)
            {
                if (rnr.Filters.IterateWrappers<DeclarativeCircuitsFilter>().FirstOrDefault() is RunnerElementWrapper c)
                    st.Circuits = c.Config["Circuits"]?.getContent()?.ToString();
                if (rnr.Filters.IterateWrappers<DeclareProtectedNamedRegion>().FirstOrDefault() is RunnerElementWrapper r)
                    st.Regions = r.Config["Patterns"]?.getContent()?.ToString();
                if (rnr.Filters.IterateWrappers<SyncByNodesFilter>().FirstOrDefault() is RunnerElementWrapper m)
                    st.Milestones = m.Config["Nodes"]?.getContent() as string[];
            }
            return st;
        }
    }
}
