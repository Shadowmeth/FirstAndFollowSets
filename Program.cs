namespace Program
{
    public class Program
    {
        private static Dictionary<string, List<string>> productionRules =
            new Dictionary<string, List<string>>();
        private static string[]? rulesList;
        private static Dictionary<string, HashSet<string>> firstSets =
            new Dictionary<string, HashSet<string>>();
        private static Dictionary<string, HashSet<string>> followSets =
            new Dictionary<string, HashSet<string>>();

        public static string[] getComponents(string prodRule)
        {
            string[] components = prodRule.Split(
                " ",
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

            return components.Length == 0 ? [prodRule] : components;
        }

        public static bool isComponentTerminal(string component)
        {
            return component != null && component[0] == '"';
        }

        public static bool isComponentNonTerminal(string component)
        {
            return component != null && component[0] == '<' && component != "<epsilon>";
        }

        public static bool isComponentEpsilon(string component)
        {
            return component != null && component == "<epsilon>";
        }

        public static bool nonTerminalHasEpsilon(string nonterminal)
        {
            bool hasEpsilon = false;
            List<string> prodRules = productionRules[nonterminal];
            foreach (string rule in prodRules)
            {
                string[] components = getComponents(rule);
                foreach (string component in components)
                {
                    if (isComponentEpsilon(component))
                    {
                        hasEpsilon = true;
                        return hasEpsilon;
                    }
                }
            }

            return hasEpsilon;
        }

        // all the components should be non terminals
        // and each of the non terminal must have epsilon production rule
        // for this to be true
        public static bool isEpsilonChained(string prodRule)
        {
            bool epsilonChain = true;
            string[] components = getComponents(prodRule);
            foreach (string component in components)
            {
                if (!(isComponentNonTerminal(component) && nonTerminalHasEpsilon(component)))
                {
                    epsilonChain = false;
                    return epsilonChain;
                }
            }

            return epsilonChain;
        }

        public static HashSet<string> calcFirstSet(string nonterminal)
        {
            HashSet<string> firstSet = new HashSet<string>();
            List<string> prodRules = productionRules[nonterminal];

            if (prodRules != null && prodRules.Count == 0)
            {
                return firstSet;
            }

            foreach (string prodRule in prodRules)
            {
                string[] components = getComponents(prodRule);
                if (
                    components.Length >= 2
                    && isComponentNonTerminal(components[0])
                    && components[0] == nonterminal
                )
                {
                    // left recursive grammar, can't compute first set of this
                    System.Console.Error.WriteLine(
                        "LEFT RECURSIVE GRAMMAR FOUND! CAN'T COMPUTE FIRST SET FOR THIS GRAMMAR"
                    );
                    System.Environment.Exit(1);
                }
                if (isComponentEpsilon(components[0]))
                {
                    firstSet.Add("<epsilon>");
                    continue;
                }
                if (isComponentTerminal(components[0]))
                {
                    firstSet.Add(components[0]);
                    continue;
                }
                if (isComponentNonTerminal(components[0]))
                {
                    // There are two possibilities in this case
                    // 1. We have chain of non terminals, in which case we should check if all of them
                    // contain epsilon so first(nonterminal) will contain epsilon
                    // 2. We have non terminal followed by a terminal/nonterminal in which case first(nonterminal) = first(components[0])
                    // OR first(nonterminal) = first(components[0]) UNION first(components[1]) if first(components[0]) contains epsilon
                    if (isEpsilonChained(prodRule))
                    {
                        firstSets[nonterminal].Add("<epsilon>");
                    }
                    if (components.Length >= 2 && !nonTerminalHasEpsilon(components[0]))
                    {
                        firstSet.UnionWith(calcFirstSet(components[0]));
                    }
                    else if (components.Length >= 2 && nonTerminalHasEpsilon(components[0]))
                    {
                        firstSet.UnionWith(calcFirstSet(components[0]));
                        firstSet.UnionWith(calcFirstSet(components[1]));
                    }
                }
            }

            return firstSet;
        }

        public static void expandProductionRules()
        {
            // if the assert fails we most likely failed to read the grammar file
            System.Diagnostics.Debug.Assert(rulesList != null && rulesList.Length != 0);

            foreach (string prodRule in rulesList)
            {
                string[] rule = prodRule.Split(
                    "->",
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                );
                System.Diagnostics.Debug.Assert(rule.Length == 2, "rule length must be 2!");
                string rhs = rule[1].Trim();
                if (!productionRules.ContainsKey(rule[0]))
                {
                    List<string> temp = new List<string>();
                    temp.Add(rhs);
                    productionRules.Add(rule[0], temp);
                }
                else
                {
                    productionRules[rule[0]].Add(rhs);
                }
            }
        }

        public static void printProductionRules()
        {
            foreach (KeyValuePair<string, List<string>> kvp in productionRules)
            {
                Console.WriteLine(kvp.Key);
                foreach (string prodRule in kvp.Value)
                {
                    Console.WriteLine("\t" + prodRule);
                }
                Console.WriteLine();
            }
        }

        public static void Main(string[] args)
        {
            rulesList = File.ReadAllLines("grammar.txt");
            expandProductionRules();
            printProductionRules();
            firstSets["<exp>"] = calcFirstSet("<exp>");
            Console.WriteLine("Printing first sets: ");
            foreach (string first in firstSets["<exp>"])
            {
                Console.WriteLine(first);
            }
        }
    }
}
