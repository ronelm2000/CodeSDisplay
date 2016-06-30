using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MQL_Phase2.DataClumps
{
    public class DataClump
    {
        List<string> Name;
        int Signature;

        public bool IsClean { get { return Name.Count < 1; } }

        private DataClump()
        {
        }

        private DataClump (string name)
        {
            this.Name = new List<string>();
            this.Name.Add(name);
        }

        internal static DataClump[] GetAllDataClumpsFrom(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            List<string> args = new List<string>();

            foreach (ParameterSyntax syntax in methodDeclarationSyntax.ParameterList.Parameters)
            {
                args.Add(syntax.Identifier.Text);
            }

            if (args.Count < 2)
            {
                DataClump newClump = new DataClump();
                newClump.Name = new List<string>();
                return new DataClump[] { newClump };
            } else
            {
                return CombinationOf(args).ToArray();
            }
        }

        private static List<DataClump> CombinationOf(List<string> args)
        {
            List<DataClump> result = new List<DataClump>();

            foreach(string[] val in Combination(args).Where((x) => x.Length > 1))
            {
                DataClump newDataClump = new DataClump();
                newDataClump.Name = val.ToList();
                result.Add(newDataClump);
            }
            return result;
        }


        static IEnumerable<string[]> Combination (IEnumerable<string> args)
        {
            foreach (string arg in args)
            {
                yield return new string[] { arg };
            }

            for (int i = 2; i < args.Count(); i++)
            {
                string[] newValue = new string[] { args.First() };
                foreach (var val in Combination(args.Skip(1).Take(args.Count() - 1))) yield return newValue.AsEnumerable().Concat(val).ToArray();
            }

            yield return args.ToArray();
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            // TODO: write your implementation of Equals() here
            if (base.Equals(obj)) return true;

            DataClump sig = (DataClump)obj;
            if (sig == null) return false;
            if ((sig.Name == null && Name != null) ||
                (Name == null && sig.Name != null)) return false;

            if (this.Signature != sig.Signature) return false;

            foreach (string n in this.Name)
            {
                if (!sig.Name.Contains(n)) return false;
            }

            return true;
        }

        /*
        public static bool operator == (DataClump one, DataClump two)
        {
            return one.Equals(two);
        }

        public static bool operator != (DataClump one, DataClump two)
        {
            return !one.Equals(two);
        }
        */

        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return base.GetHashCode();
        }
    }
}
