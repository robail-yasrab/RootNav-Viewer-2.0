using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RootNav.Data
{
    /// <summary>
    /// Lightweight version of the root class. Used in measurements to preserve a root hierarchy for a plant.
    /// </summary>
    public class PlantInfo : IEnumerable<RootInfo>
    {
        public String RsmlID { get; set; }
        public String Label { get; set; }
        public String RelativeID { get; set; }
        public String Tag { get; set; }
        public DateTime Stamp { get; set; }
        public bool Complete { get; set; }
        public List<RootInfo> Roots { get; set; }
        public List<Annotation> Annotations { get; set; }
        public List<Property> Properties { get; set; }

        #region IEnumerable<RootInfo>
        public IEnumerator<RootInfo> GetEnumerator()
        {
            Queue<RootInfo> rootStack = new Queue<RootInfo>();

            foreach (RootInfo r in Roots)
            {
                rootStack.Enqueue(r);

                while (rootStack.Count > 0)
                {
                    RootInfo current = rootStack.Dequeue();
                    yield return current;

                    if (current.Children != null)
                    {
                        foreach (RootInfo child in current.Children)
                        {
                            rootStack.Enqueue(child);
                        }
                    }
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        public static PlantInfo CreateTree(String tag, DateTime stamp, bool complete, List<RootInfo> rootData)
        {
            // Count plants and obtain reference numbers
            Dictionary<String, List<RootInfo>> plantLists = new Dictionary<string, List<RootInfo>>();

            String plantID = rootData[0].RelativeID;
            List<RootInfo> currentRootList = rootData;
            PlantInfo currentPlant = new PlantInfo() { RelativeID = plantID, Label = rootData[0].Label, Tag = tag, Stamp = stamp, Complete = complete, Roots = new List<RootInfo>() };
            Dictionary<String, RootInfo> sortedRoots = new Dictionary<string, RootInfo>();

            String[] arr = currentRootList.Select(a => a.RelativeID).ToArray();
             
            foreach (RootInfo r in currentRootList)
            {
                if (r.RelativeID.IndexOf('.') < 0)
                {
                    continue;
                }
                
                String parentTag = r.RelativeID.Substring(0, r.RelativeID.LastIndexOf('.'));

                if (sortedRoots.ContainsKey(parentTag))
                {
                    if (sortedRoots[parentTag].Children == null)
                    {
                        sortedRoots[parentTag].Children = new List<RootInfo>();
                    }

                    sortedRoots[parentTag].Children.Add(r);
                    sortedRoots.Add(r.RelativeID, r);
                }
                else
                {
                    if (sortedRoots.ContainsKey(r.RelativeID))
                    {
                        // This should not happen anymore, duplicate tags are handled before PlantInfo objects are created.
                        System.Windows.MessageBox.Show("The tag " + tag + " has been found twice. The second plant has been ignored. This should be fixed in the database.", "Duplicate plant tag found");
                        break;
                    }
                    else
                    {
                        sortedRoots.Add(r.RelativeID, r);
                        currentPlant.Roots.Add(r);
                    }
                }
            }

            return currentPlant;
        }

        public RootInfo GetParent(RootInfo root)
        {
            String parentRelativeID = GetParentTag(root.RelativeID);
            foreach (RootInfo current in this)
            {
                if (current.RelativeID == parentRelativeID)
                {
                    return current;
                }
            }
            return null;
        }

        public string GetParentTag(string tag)
        {
            return tag.Substring(0, tag.LastIndexOf('.'));
        }
    }
}
