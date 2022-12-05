using System.Threading;

namespace Task
{
    class Program
    {
        public static void Main(string[] args)
        {
            void Test1()
            {
                var stack = new ConcurrentStack<string>();
                var thread1 = new Thread(() =>
                {
                    for (var i = 0; i < 100; i++)
                        stack.Push(i.ToString());
                });
                var thread2 = new Thread(() =>
                {
                    for (var i = 0; i < 100; i++)
                        stack.Push(i.ToString());
                });
                thread1.Start();
                thread2.Start();
                thread1.Join();
                thread2.Join();
                Console.WriteLine(stack.Count);
            }

            void Test2()
            {
                var stack = new ConcurrentStack<string>();
                var thread1 = new Thread(() =>
                {
                    for (var i = 0; i < 100; i++)
                        stack.Push(i.ToString());
                });
                var thread2 = new Thread(() =>
                {
                    var item = "";
                    for (var i = 0; i < 100; i++)
                    {
                        Console.WriteLine(stack.TryPop(out item));
                        Console.WriteLine(stack.Count);
                    };
                });
                thread1.Start();
                thread2.Start();
            }

            Test1();
            Test2();
        }

        public class StackNode<T>
        {
            public int Count;
            public T Value { get; }
            public StackNode<T> Previous { get; set; }

            public StackNode(T value)
            {
                Value = value;
            }
        }

        public interface IStack<T>
        {
            void Push(T item);
            bool TryPop(out T item);
            int Count { get; }
        }

        public class ConcurrentStack<T> : IStack<T>
        {
            private StackNode<T> currentNode;
            public int Count => currentNode is null ? 0 : currentNode.Count + 1;

            public void Push(T item)
            {
                var spinWait = new SpinWait();
                while (true)
                {
                    var node = new StackNode<T>(item)
                    {
                        Previous = currentNode,
                        Count = currentNode is null ? 0 : currentNode.Count + 1
                    };

                    if (Interlocked.CompareExchange(ref currentNode, node, node.Previous) == node.Previous)
                        break;

                    spinWait.SpinOnce();
                }
            }

            public bool TryPop(out T item)
            {
                var spinWait = new SpinWait();
                while (true)
                {
                    var node = currentNode;

                    if (node == null)
                    {
                        item = default;
                        return false;
                    }

                    if (Interlocked.CompareExchange(ref currentNode, node.Previous, node) == node)
                    {
                        item = node.Value;
                        return true;
                    }

                    spinWait.SpinOnce();
                }
            }
        }
    }
}