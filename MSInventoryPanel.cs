using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public class MSInventoryPanel : MachineEntity//, PowerConsumerInterface
{
    public float mrMaxPower = 100f;
    public float mrMaxTransferRate = 100f;
    private bool mbLinkedToGO;
    private float mrReadoutScale;
    public float mrViewTime;
    private GameObject ViewObject;
    private float mrUpdateVisuals;
    private float mrScanDelay;
    public float mrUptime;
    public int mnCurrentPlayers;
    public int mnMaxPlayers;
    public int mnNumMachines;
    public float mrMachineLoadPercent;
    public int mnNumMobs;
    public float mrMobLoadPercent;
    public float mrServerLoadPercent;
    public int mnSegmentsToSent;
    public int BarsMin;
    public int OresMin;
    public int PowerMin;
    public float mrCurrentPower;
    private MassStorageCrate massStorageCrate;
    public int[] quant;
    public string[] itemnames;
    private int scrollindex = 0;
    private bool autoscroll = true;
    private float paneldebounce = 0.0f;

    public MSInventoryPanel(Segment segment, long x, long y, long z, ushort cube, byte flags, ushort lValue, bool loadFromDisk)
      : base(eSegmentEntity.Mod, SpawnableObjectEnum.ServerMonitor, x, y, z, cube, flags, lValue, Vector3.zero, segment)
    {
        this.mbNeedsLowFrequencyUpdate = true;
        this.mbNeedsUnityUpdate = true;
    }

    public override void DropGameObject()
    {
        this.ViewObject = (GameObject)null;
        base.DropGameObject();
        this.mbLinkedToGO = false;
    }

    public override string GetPopupText()
    {
        this.paneldebounce -= Time.deltaTime;
        string lstr1 = "";
        string lstr2;
        if (this.massStorageCrate == null)
            lstr1 = "Mass Storage Inventory Panel\nSearching for adjacent Mass Storage Crate...";
        else
        {
            if (Input.GetButton("Extract") && this.paneldebounce < 0.0f)
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    if (this.autoscroll)
                        this.scrollindex -= 2;
                    else
                        this.scrollindex--;
                }
                else
                {
                    if (this.autoscroll)
                        this.scrollindex -= 9;
                    else
                        this.scrollindex -= 8;
                }

                this.paneldebounce = 0.5f;
            }
            if (Input.GetButton("Interact") && this.paneldebounce < 0.0f)
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                    this.scrollindex++;
                else
                    this.scrollindex += 8;
                this.paneldebounce = 0.5f;
            }
            if (Input.GetButton("Store") && this.paneldebounce < 0.0f)
            {
                if (this.autoscroll)
                    this.autoscroll = false;
                    
                else
                    this.autoscroll = true;
                this.paneldebounce = 0.5f;
            }

            if (this.autoscroll)
                lstr2 = " off";
            else
                lstr2 = " on";
            lstr1 = "Mass Storage Inventory Panel\n(Q) to scroll up\n(E) to scroll down\n(T) to toggle auto-scrolling" + lstr2;
        }
        this.mrViewTime = 1f;
        return lstr1;
    }

    public override void UnityUpdate()
    {
        if (!this.mbLinkedToGO)
        {
            if (this.mWrapper == null || !this.mWrapper.mbHasGameObject)
                return;
            if (this.mWrapper.mGameObjectList == null)
                Debug.LogError((object)"TS missing game object #0?");
            if ((object)this.mWrapper.mGameObjectList[0].gameObject == (object)null)
                Debug.LogError((object)"TS missing game object #0 (GO)?");
            this.ViewObject = Extensions.Search(this.mWrapper.mGameObjectList[0].transform, "_Readout").gameObject;
            this.ViewObject.SetActive(false);
            this.ViewObject.transform.localScale = Vector3.zero;
            this.mbLinkedToGO = true;
            this.mrReadoutScale = 0.0f;
            this.mrUpdateVisuals = -1f;
        }
        this.mrUpdateVisuals -= Time.deltaTime;
        if ((double)this.mrViewTime > 0.0)
        {
            this.ViewObject.SetActive(true);
            this.mrReadoutScale += Time.deltaTime;
            if ((double)this.mrReadoutScale > 1.0)
                this.mrReadoutScale = 1f;
        }
        else
        {
            this.mrReadoutScale *= 0.95f;
            if ((double)this.mrReadoutScale < 0.100000001490116)
            {
                this.mrReadoutScale = 0.0f;
                this.ViewObject.SetActive(false);
            }
        }
        this.ViewObject.transform.localScale = new Vector3(this.mrReadoutScale * 2f, this.mrReadoutScale * 2f, this.mrReadoutScale * 2.5f);
        if ((double)this.mrUpdateVisuals >= 0.0 || (double)this.mrReadoutScale <= 0.0)
            return;

        //Display string definition
        if (itemnames == null || quant == null)
        {
            if (massStorageCrate == null)
            {
                this.mrUpdateVisuals = 1f;
                Extensions.Search(this.ViewObject.transform, "ReadoutText").GetComponent<TextMesh>().text = "Searching for Mass Storage Crate...";
                Extensions.Search(this.ViewObject.transform, "ReadoutText_DS").GetComponent<TextMesh>().text = "Searching for Mass Storage Crate...";
            }
            else
            {
                this.mrUpdateVisuals = 1f;
                Extensions.Search(this.ViewObject.transform, "ReadoutText").GetComponent<TextMesh>().text = "Scanning inventory...";
                Extensions.Search(this.ViewObject.transform, "ReadoutText_DS").GetComponent<TextMesh>().text = "Scanning inventory...";
            }
            return;
        }
        
        //Roll back to the beginning
        if (this.scrollindex > this.itemnames.Count() - 1)
            this.scrollindex = 0;
        //Or to the end
        if (this.scrollindex < 0)
            this.scrollindex += this.itemnames.Count();

        //Don't allowing scrolling if it all fits on screen
        if (this.itemnames.Count() <= 8)
            this.scrollindex = 0;

        string str1 = "Mass Storage Inventory: ";
        string str2 = "";
        string str4 = "";
        int printline = 0;
        for (int index = 0; index < 8; index++)
        {
            printline = index + scrollindex;
            //Debug.Log("index: " + index + " Scroll index: " + scrollindex);
            if (index + scrollindex >= itemnames.Count() && itemnames.Count() > 8)
                printline -= itemnames.Count();
            if (printline >= itemnames.Count() || printline >= quant.Count())
            {
                //Debug.Log("Tried to print an item that doesn't exist in the list! GET OUT!");
                break;
            }
            str4 = itemnames[printline].ToString();
            str2 = str2 + "\n" + quant[printline].ToString("N0") + str4.Substring(1) ;
        }
        string str3 = string.Concat(new object[2]
        {
        (object) (str1),
        (object) (str2)
        });
        this.mrUpdateVisuals = 1f;
        Extensions.Search(this.ViewObject.transform, "ReadoutText").GetComponent<TextMesh>().text = str3;
        Extensions.Search(this.ViewObject.transform, "ReadoutText_DS").GetComponent<TextMesh>().text = str3;

        if (this.autoscroll)
            this.scrollindex++;
    }

    public override void LowFrequencyUpdate()
    {
        this.mrViewTime -= LowFrequencyThread.mrPreviousUpdateTimeStep;
        if ((double)this.mrScanDelay > 0.0)
        {
            this.mrScanDelay -= LowFrequencyThread.mrPreviousUpdateTimeStep;
        }
        else
        {
            //ScanDelay sets the refresh interval
            this.mrScanDelay = 1f;
            //Clear arrays from last use so we don't have phantom items
            ItemBase pickeditem = null;
            string itemname = "";
            Dictionary<string, int> items = new Dictionary<string, int>();


            //Loop over all crates and collect all items into a list
            if (massStorageCrate != null)
            {
                for (int index = 0; index < this.massStorageCrate.mConnectedCrates.Count+1; ++index)
                {
                    for (int index2 = 0; index2 < massStorageCrate.STORAGE_CRATE_SIZE; ++index2)
                    {
                        if (index == this.massStorageCrate.mConnectedCrates.Count) //Center crate!
                        {
                            if (massStorageCrate.mItems[index2] != null)
                                pickeditem = (massStorageCrate.mItems[index2]);
                        }
                        else if (massStorageCrate.mConnectedCrates[index].mItems[index2] != null)
                        {
                            pickeditem = (massStorageCrate.mConnectedCrates[index].mItems[index2]);
                        }
                        if (pickeditem != null)
                        {
                            itemname = pickeditem.ToString();
                            if (!items.ContainsKey(itemname))
                            {
                                items.Add(itemname, 1);
                            }
                            else
                            {
                                items[itemname] = (int)items[itemname] + 1;
                            }
                            pickeditem = null;
                        }   
                    }
                }
            }
            //No mass storage crate associated... find one!
            else
            {
                SearchForCrateNeighbours(this.mnX, this.mnY, this.mnZ);
            }


            //Group like items in the list and output the quantities to arrays
            int index3 = 0;
            if (items.Count == 0)
                return;
            else
            {
                this.itemnames = new string[items.Count];
                this.quant = new int[items.Count];
                var sortlist = items.Keys.ToList();
                sortlist.Sort();

                foreach (var key in sortlist)
                {
                    this.itemnames[index3] = key;
                    this.quant[index3] = items[key];
                    index3++;
                }
            }
        }
    }

    private void SearchForCrateNeighbours(long x, long y, long z)
    {
        for (int index = 0; index < 6; ++index)
        {
            //Debug.Log("Searching for crate");
            long x1 = x;
            long y1 = y;
            long z1 = z;
            if (index == 0)
                --x1;
            if (index == 1)
                ++x1;
            if (index == 2)
                --y1;
            if (index == 3)
                ++y1;
            if (index == 4)
                --z1;
            if (index == 5)
                ++z1;
            Segment segment = this.AttemptGetSegment(x1, y1, z1);
            if (segment == null)
            {
                segment = WorldScript.instance.GetSegment(x1, y1, z1);
                if (segment == null)
                {
                    Debug.Log((object)"SearchForCrateNeighbours did not find segment");
                    continue;
                }
            }
            if ((int)segment.GetCube(x1, y1, z1) == 527)
            {
                MassStorageCrate massStorageCrate = segment.FetchEntity(eSegmentEntity.MassStorageCrate, x1, y1, z1) as MassStorageCrate;
                if (massStorageCrate == null)
                    return;
                this.massStorageCrate = massStorageCrate.GetCenter();
            }
        }
    }
}
