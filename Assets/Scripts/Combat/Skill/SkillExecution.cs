namespace Combat.Skill
{
    /// <summary>
    /// 技能执行实例 — 持有当前执行进度，由 SkillSystem.Tick 每帧驱动。
    /// 无 Unity 依赖，可单元测试。
    /// </summary>
    public class SkillExecution
    {
        public bool IsComplete { get; private set; }

        private SkillNodeBase _currentNode;
        private readonly SkillContext _ctx;
        private readonly Core.HitPipeline _pipeline;
        private readonly Buff.BuffSystem _buffSystem;

        public SkillExecution(SkillGraph graph, SkillContext ctx,
            Core.HitPipeline pipeline, Buff.BuffSystem buffSystem)
        {
            _ctx = ctx;
            _pipeline = pipeline;
            _buffSystem = buffSystem;
            _currentNode = graph.GetEntryNode();
            _currentNode?.OnEnter(ctx);
        }

        public void Tick(float delta)
        {
            if (IsComplete || _currentNode == null) { IsComplete = true; return; }

            bool done = _currentNode.Tick(delta, _ctx, _pipeline, _buffSystem);
            if (!done) return;

            // 条件节点走分支端口，其余走默认 output
            SkillNodeBase next = _currentNode is ConditionNode cond
                ? cond.GetBranchNode(_ctx)
                : _currentNode.GetNextNode();

            if (next == null) { IsComplete = true; return; }
            _currentNode = next;
            _currentNode.OnEnter(_ctx);
        }
    }
}
