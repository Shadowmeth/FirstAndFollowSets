namespace Program
{
    public class Program
    {
        private static Dictionary<String, List<String>> productionRules =
            new Dictionary<String, List<String>>();
        private static String[]? rulesList;

        public static void expandProductionRules()
        {
            foreach (String prodRule in rulesList)
            {
                String[] rule = prodRule.Split(
                    "->",
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                );
                System.Diagnostics.Debug.Assert(rule.Length == 2, "rule length must be 2!");
                String rhs = rule[1].Trim();
                if (!productionRules.ContainsKey(rule[0]))
                {
                    List<String> temp = new List<String>();
                    temp.Add(rhs);
                    productionRules.Add(rule[0], temp);
                }
                else
                {
                    productionRules[rule[0]].Add(rhs);
                }
            }
        }

        public static void Main(String[] args)
        {
            rulesList = File.ReadAllLines("grammar.txt");
            expandProductionRules();

            foreach (KeyValuePair<String, List<String>> kvp in productionRules)
            {
                Console.WriteLine(kvp.Key);
                foreach (String prodRule in kvp.Value)
                {
                    Console.WriteLine("\t" + prodRule);
                }
            }
        }
    }
}
