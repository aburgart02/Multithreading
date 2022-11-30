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
            private int _count;
            public int Count => _count;
            private StackNode<T> currentNode;

            public void Push(T item)
            {
                var spinWait = new SpinWait();
                while (true)
                {
                    var node = new StackNode<T>(item) { Previous = currentNode };

                    if (Interlocked.CompareExchange(ref currentNode, node, node.Previous) == node.Previous)
                        break;

                    spinWait.SpinOnce();
                }
                Interlocked.Increment(ref _count);
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
                        Interlocked.Decrement(ref _count);
                        return true;
                    }

                    spinWait.SpinOnce();
                }
            }
        }
    }
}