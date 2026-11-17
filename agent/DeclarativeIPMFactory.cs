#nullable disable
using pm4h.svelta;
using pm4h.svelta.chains;
using PMAppSVELTA.src;

namespace PMAppSVELTA.DeclarativeIPM
{
    /// <summary>Entry point: the whole agent as a Chain, and as an entry of the chat's agent list.</summary>
    public static class DeclarativeIPMFactory
    {
        /// <summary>agent (one model call + validation) | apply the operation. Without arguments it uses Ollama with the external configuration.</summary>
        public static Chain Create(IDeclarativeIPMLlm llm = null) =>
            new DeclarativeIPMAgent { Llm = llm ?? new DeclarativeIPMOllama() } | new DeclarativeIPMDispatch();

        /// <summary>For <c>SVELTAHelper.ChainsInstalled.Add(DeclarativeIPMFactory.AsChainAgent());</c></summary>
        public static ChainAgent AsChainAgent(IDeclarativeIPMLlm llm = null) =>
            new ChainAgent(4, "Experiment Viewer", "Declarative IPM", Create(llm));
    }
}
