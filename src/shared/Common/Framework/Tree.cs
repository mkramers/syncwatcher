using System;
using System.Collections.Generic;

namespace Common.Framework
{
    public class Tree<TNode>
    {
        public TNode Data { get; set; }
        public List<Tree<TNode>> Children { get; set; }

        public Tree()
        {
            Data = default(TNode);
            Children = new List<Tree<TNode>>();
        }

        public Tree(TNode _data)
        {
            Data = _data;
            Children = new List<Tree<TNode>>();
        }

        public void AddChild(TNode _data)
        {
            Children.Add(new Tree<TNode>(_data));
        }

        public void AddNode(Tree<TNode> _data)
        {
            Children.Add(_data);
        }

        public Tree<TNode> GetChild(int _i)
        {
            Tree<TNode> child = null;

            if (_i < Children.Count && _i >= 0)
            {
                child = Children[_i];
            }

            return child;
        }

        public static Tree<TNode> Clone(Tree<TNode> _source)
        {
            Tree<TNode> copy = new Tree<TNode>(_source.Data);

            if (_source.Children.Count == 0)
            {
                copy.Data = _source.Data;
            }
            else
            {
                foreach (Tree<TNode> t in _source.Children)
                {
                    copy.AddNode(Clone(t));
                }
            }

            return copy;
        }

        public static TNode Find(Tree<TNode> _tree, Predicate<TNode> _match)
        {
            TNode found = default(TNode);
            bool isFound = false;

            //check the current node for a match
            if (_tree.Data != null)
            {
                if (_match(_tree.Data))
                {
                    found = _tree.Data;
                    isFound = true;
                }
            }

            if (!isFound)
            {
                foreach (Tree<TNode> t in _tree.Children)
                {
                    found = Find(t, _match);

                    if (found != null)
                    {
                        break;
                    }
                }
            }

            return found;
        }

        public static void PrintTree(Tree<TNode> _tree, int _indent)
        {
            foreach (Tree<TNode> t in _tree.Children)
            {
                if (t != null && t.Data != null)
                {
                    string data = t.Data.ToString();

                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        Console.WriteLine(data.PadLeft(data.Length + 3 * _indent + 1, '>'));
                    }
                }
                else
                {
                    Console.WriteLine("root");
                }

                PrintTree(t, _indent + 1);
            }
        }

        public static void PrintTreeChildren(Tree<TNode> _tree, string _tag)
        {
            List<string> childlessNodes = new List<string>();
            childlessNodes = GetListOfChildlessNodes(_tree, childlessNodes, _tag);

            foreach (string t in childlessNodes)
            {
                Console.WriteLine(t);
            }
        }

        public static List<string> GetListOfChildlessNodes(Tree<TNode> _tree, List<string> _list, string _tag)
        {
            if (_tree.Children.Count != 0)
            {
                foreach (Tree<TNode> t in _tree.Children)
                {
                    if (t?.Children != null)
                    {
                        string data = t.Data.ToString();

                        GetListOfChildlessNodes(t, _list, _tag + "/" + data);
                    }
                }
            }
            else
            {
                if (_tree.Data != null)
                {
                    string data = _tree.Data.ToString();
                    _list.Add(_tag + "/" + data);
                }
            }

            return _list;
        }

        public static int GetNumberOfChildren(Tree<TNode> _tree, int _count)
        {
            int numberOfChildren = _count;

            if (_tree.Children.Count != 0)
            {
                foreach (Tree<TNode> t in _tree.Children)
                {
                    if (t?.Children != null)
                    {
                        numberOfChildren = GetNumberOfChildren(t, numberOfChildren);
                    }
                }
            }
            else
            {
                if (_tree.Data != null)
                {
                    numberOfChildren++;
                }
            }

            return numberOfChildren;
        }
    }
}
