using System;
using Cake.Core;
using Cake.Core.Annotations;
using P4Commands;

namespace CakeExtensions
{
    public static class SampleExtensions
    {
        [CakeMethodAlias]
        public static int GetMagicNumber(this ICakeContext context, bool value)
        {
            return value ? int.MinValue : int.MaxValue;
        }

        [CakeMethodAlias]
        public static int GetMagicNumberOrDefault(this ICakeContext context, bool value, Func<int> defaultValueProvider = null)
        {
            if (value)
            {
                return int.MinValue;
            }

            return defaultValueProvider == null ? int.MaxValue : defaultValueProvider();
        }

        [CakePropertyAlias]
        public static int TheAnswerToLife(this ICakeContext context)
        {
            return 42;
        }
    }

    public static class P4Extensions
    {
        [CakeMethodAlias]
        public static string GetCurrentChangeList(this ICakeContext _context)
        {
            var changeList = Commands.GetChangelist();
            return changeList?.ToString();
        }

        [CakeMethodAlias]
        public static string GetCurrentChangeList(this ICakeContext _context, string _stageFile)
        {
            var changeList = Commands.GetChangelist();
            return changeList?.ToString();
        }
    }
}