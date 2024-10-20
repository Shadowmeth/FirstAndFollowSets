namespace Program
{
    public class Program
    {
        private static Dictionary<string, List<string>> ProductionRules =
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

        // see if the specific nonterminal occurs in the production rule
        public static bool productionRuleHasNonTerminal(string nonterminal, string prodRule)
        {
            string[] components = getComponents(prodRule);
            return components.Contains(nonterminal);
        }

        public static List<string> getProdRulesWithNonTerminal(string nonterminal)
        {
            List<string> result = new List<string>();
            foreach (KeyValuePair<string, List<string>> kvp in ProductionRules)
            {
                string lhs = kvp.Key;
                List<string> rules = ProductionRules[lhs];
                foreach (string prodRule in rules)
                {
                    if (productionRuleHasNonTerminal(nonterminal, prodRule))
                    {
                        result.Add(prodRule);
                    }
                }
            }

            return result;
        }

        public static bool nonTerminalHasEpsilon(string nonterminal)
        {
            bool hasEpsilon = false;
            List<string> prodRules = ProductionRules[nonterminal];
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
            List<string> prodRules = ProductionRules[nonterminal];

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
                    else if (
                        components.Length >= 2
                        && nonTerminalHasEpsilon(components[0])
                        && isComponentNonTerminal(components[1])
                    )
                    {
                        firstSet.UnionWith(calcFirstSet(components[0]));
                        firstSet.UnionWith(calcFirstSet(components[1]));
                    }
                    else if (
                        components.Length >= 2
                        && nonTerminalHasEpsilon(components[0])
                        && isComponentTerminal(components[1])
                    )
                    {
                        firstSet.UnionWith(calcFirstSet(components[0]));
                        firstSet.Add(components[1]);
                    }
                    else if (components.Length == 1)
                    {
                        firstSet.UnionWith(calcFirstSet(components[0]));
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
                if (!ProductionRules.ContainsKey(rule[0]))
                {
                    List<string> temp = new List<string>();
                    temp.Add(rhs);
                    ProductionRules.Add(rule[0], temp);
                }
                else
                {
                    ProductionRules[rule[0]].Add(rhs);
                }
            }
        }

        public static void calcFirstSets()
        {
            foreach (KeyValuePair<string, List<string>> kvp in ProductionRules)
            {
                firstSets.Add(kvp.Key, new HashSet<string>());
            }

            foreach (KeyValuePair<string, List<string>> kvp in ProductionRules)
            {
                firstSets[kvp.Key].UnionWith(calcFirstSet(kvp.Key));
            }

            foreach (KeyValuePair<string, List<string>> kvp in ProductionRules)
            {
                if (!nonTerminalHasEpsilon(kvp.Key))
                {
                    firstSets[kvp.Key].Remove("<epsilon>");
                }
            }
        }

        public static void printProductionRules()
        {
            ConsoleColor origColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;

            Console.WriteLine("PRINTING PRODUCTION RULES!");
            foreach (KeyValuePair<string, List<string>> kvp in ProductionRules)
            {
                Console.WriteLine(kvp.Key);
                foreach (string prodRule in kvp.Value)
                {
                    Console.WriteLine("\t" + prodRule);
                }
                Console.WriteLine();
            }
            Console.ForegroundColor = origColor;
        }

        public static void printFirstSets()
        {
            ConsoleColor origColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("PRINTING FIRST SETS!");
            foreach (KeyValuePair<string, HashSet<string>> kvp in firstSets)
            {
                Console.WriteLine(kvp.Key);
                foreach (string first in kvp.Value)
                {
                    Console.WriteLine("\t" + first);
                }
                Console.WriteLine();
            }
            Console.ForegroundColor = origColor;
        }

        public static int findNTIndex(string[] components, string nt)
        {
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == nt)
                {
                    return i;
                }
            }

            System.Console.Error.WriteLine("UNREACHABLE CODE PATH REACHED IN findNTIndex()");
            System.Environment.Exit(1);
            return -1; // NOTE: this will never be reached
        }

        public static string getNTForProductionRule(string productionRule)
        {
            foreach (KeyValuePair<string, List<string>> kvp in ProductionRules)
            {
                foreach (string rule in ProductionRules[kvp.Key])
                {
                    if (productionRule == rule)
                    {
                        return kvp.Key;
                    }
                }
            }

            System.Console.Error.WriteLine(
                "UNREACHABLE CODE PATH REACHED IN getNTForProductionRule()"
            );
            System.Environment.Exit(1);
            return ""; // NOTE: this will never be reached
        }

        public static bool firstSetContainsEpsilon(string nonterminal)
        {
            return firstSets[nonterminal].Contains("<epsilon>");
        }

        public static HashSet<string> calcFollowSet(
            string nonterminal,
            Dictionary<string, List<string>> occurrences
        )
        {
            HashSet<string> followSet = new HashSet<string>();
            // 2 possibilities:
            // 1. if there is a production of form <B> -> <alpha> <A> <beta> then First(beta) - { <epsilon> } is in Follow(A)
            // unless <beta> is a terminal then it comes as is in Follow(A)
            // 2. if there is a production of form <B> -> <alpha> <A> <beta> such that <epsilon> is in First(beta) then Follow(A)
            // contains Follow(B)

            foreach (KeyValuePair<string, List<string>> kvp in ProductionRules)
            {
                string nt = nonterminal; // nonterminal whose follow set we need to find
                List<string> occurrenceRules = occurrences[nt]; // all the production rules in which nt occurs at RHS

                // couldn't filter out bad production rules in calcFollowSets
                // so we just return the empty Dictionary here
                if (occurrenceRules.Count == 0)
                {
                    return followSet;
                }
                if (occurrenceRules.Count == 1 && occurrenceRules[0] == nt)
                {
                    return followSet;
                }

                foreach (string rule in occurrenceRules)
                {
                    string[] components = getComponents(rule);
                    // find index of nt in components
                    // this will never be -1 because we are ensuring only searching production rules
                    // which contain nt
                    int index = findNTIndex(components, nt);
                    string component = components[index];
                    if (index == components.Length - 1)
                    {
                        // nt is in the end of production rule
                        // Follow(nt) = Follow(lhs)
                        string lhs = getNTForProductionRule(rule);
                        if (lhs == nt)
                        {
                            continue;
                        }
                        // lhs is the start symbol
                        if (followSets[lhs].Contains("$"))
                        {
                            followSet.Add("$");
                        }
                        followSet.UnionWith(calcFollowSet(lhs, occurrences));
                    }
                    else if (
                        index == components.Length - 2
                        && isComponentNonTerminal(components[index + 1])
                    )
                    {
                        if (firstSetContainsEpsilon(components[index + 1]))
                        {
                            followSet.UnionWith(firstSets[components[index + 1]]);
                            string lhs = getNTForProductionRule(rule);
                            followSet.UnionWith(firstSets[lhs]);
                            followSet.Remove("<epsilon>");
                        }
                        else
                        {
                            followSet.UnionWith(firstSets[components[index + 1]]);
                        }
                    }
                    else if (
                        index == components.Length - 2
                        && isComponentTerminal(components[index + 1])
                    )
                    {
                        followSet.Add(components[index + 1]);
                    }
                    else if (isComponentTerminal(components[index + 1]))
                    {
                        followSet.Add(components[index + 1]);
                    }
                    else if (isComponentNonTerminal(components[index + 1]))
                    {
                        if (firstSetContainsEpsilon(components[index + 1]))
                        {
                            // NOTE: possible issue:
                            // what if there are two non terminals with firstSet containing epsilon?
                            followSet.UnionWith(firstSets[components[index + 1]]);
                            followSet.Remove("<epsilon>");
                        }
                        else
                        {
                            followSet.UnionWith(firstSets[components[index + 1]]);
                        }
                    }
                }
            }

            return followSet;
        }

        public static void calcFollowSets()
        {
            // for each nonterminal we need to get the production rules in which that
            // nonterminal occurs and then calculate the follow set
            Dictionary<string, List<string>> occurrences = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, List<string>> kvp in ProductionRules)
            {
                occurrences.Add(kvp.Key, getProdRulesWithNonTerminal(kvp.Key));
            }

            foreach (KeyValuePair<string, List<string>> kvp in ProductionRules)
            {
                followSets.Add(kvp.Key, new HashSet<string>());
            }

            // foreach (KeyValuePair<string, List<string>> kvp in occurrences)
            // {
            //     Console.WriteLine(kvp.Key);
            //     List<string> rules = kvp.Value;
            //     foreach (string rule in rules)
            //     {
            //         Console.WriteLine("\t" + rule);
            //     }
            // }
            // Console.WriteLine();

            HashSet<string> temp = new HashSet<string>();
            temp.Add("$");
            followSets[ProductionRules.Keys.ElementAt(0)].UnionWith(temp);

            foreach (KeyValuePair<string, List<string>> kvp in ProductionRules)
            {
                followSets[kvp.Key].UnionWith(calcFollowSet(kvp.Key, occurrences));
            }
        }

        public static void printFollowSets()
        {
            ConsoleColor origColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("PRINTING FOLLOW SETS!");
            foreach (KeyValuePair<string, HashSet<string>> kvp in followSets)
            {
                Console.WriteLine(kvp.Key);
                foreach (string follow in kvp.Value)
                {
                    Console.WriteLine("\t" + follow);
                }
                Console.WriteLine();
            }
            Console.ForegroundColor = origColor;
        }

        public static void Main(string[] args)
        {
            rulesList = File.ReadAllLines("grammar.txt");
            expandProductionRules();
            printProductionRules();
            calcFirstSets();
            printFirstSets();
            calcFollowSets();
            printFollowSets();
        }
    }
}
