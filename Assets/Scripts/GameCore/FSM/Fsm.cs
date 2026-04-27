using System;
using System.Collections.Generic;
using Tools.Log;
using Tools.ReferencePool;

namespace GameCore.FSM{
    public class Fsm<T> : FsmBase, IReference, IFsm<T> where T : class{
        private string _name;
        private T _owner;
        private Blackboard _blackboard;
        private bool _isRunning;
        private bool _isDestroyed;
        private float _currentStateTime;

        public string Name => _name;
        public T Owner => _owner;

        public override int FsmStateCount => StateDic.Count;
        public override bool IsRunning => _isRunning;
        public override bool IsDestroyed => _isDestroyed;
        public override string CurrentStateName => CurrentState?.name;
        public override float CurrentStateTime => _currentStateTime;
        public FsmState<T> CurrentState{ get; private set; }

        protected readonly Dictionary<Type, FsmState<T>> StateDic = new();

        protected virtual void Init(){
            _blackboard = ReferencePool.Acquire<Blackboard>();
        }

        public static Fsm<T> Creat(string name, T owner, List<FsmState<T>> states){
            var fsm = ReferencePool.Acquire<Fsm<T>>();
            fsm._name = name;
            fsm._owner = owner;
            fsm.Init();
            foreach (var state in states){
                fsm.StateDic.Add(state.GetType(), state);
                state.OnInit(fsm);
            }

            return fsm;
        }

        public void Start<TState>() where TState : FsmState<T>{
            if (IsRunning){
                throw new Exception("状态机正在运行，无法再次启动");
            }

            var state = GetState<TState>();

            CurrentState = state ?? throw new Exception("不存在该状态");
            _currentStateTime = 0f;
            CurrentState.OnEnter(this);
        }

        public void Stop(){
            CurrentState.OnExit();
            
            foreach (var (type, state) in StateDic){
                state.OnDestroy();
            }
            StateDic.Clear();
            ReferencePool.Release(this);
            
            _name = string.Empty;
            _owner = null;
            _blackboard.Clear();
            _blackboard = null;
            _isRunning = false;
            _isDestroyed = true;
        }

        public FsmState<T> GetState(Type state){
            if (!StateDic.ContainsKey(state)){
                HLog.LogE("获取的状态不存在");
            }

            return StateDic[state];
        }

        public FsmState<T> GetState<TFsmState>() where TFsmState : FsmState<T>{
            return GetState(typeof(TFsmState));
        }

        public void ChangeState<TFsmState>() where TFsmState : FsmState<T>{
            CurrentState?.OnExit();
            CurrentState = GetState<TFsmState>();
            CurrentState?.OnEnter(this);
        }

        public void ChangeState<TFsmState>(TFsmState state) where TFsmState : FsmState<T>{
            CurrentState?.OnExit();
            CurrentState = state;
            CurrentState?.OnEnter(this);
        }

        public void SetData<TValueType>(string name, TValueType value){
            _blackboard.SetValue(name, value);
        }

        public TData GetData<TData>(string name){
            return _blackboard.GetValue<TData>(name);
        }

        public override void OnTick(TimeSpan deltaTime){
            CurrentState?.OnTick(deltaTime);
        }

        public void Clear(){
            Stop();
        }
    }
}