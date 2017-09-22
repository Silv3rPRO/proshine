using System;
using MoonSharp.Interpreter;

namespace PROBot.Scripting
{
    public class Invoker
    {
        public DynValue[] Args;
        public bool Called;
        public DynValue Function;
        public LuaScript Script;
        public DateTime Time;

        public void Call()
        {
            Called = true;
            Script.Invoke(Function, 0, Args);
        }
    }
}