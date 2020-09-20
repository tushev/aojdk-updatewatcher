using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace AJ_UpdateWatcher
{
    static class AppDataPersistence
    {
        static public string MachineDataPath {
            get
            {
                var folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "AdoptiumUpdateWatcher"
                    );

                if (!Directory.Exists(folder))
                {
                    try
                    {
                        Directory.CreateDirectory(folder);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"There was an error: [ {ex.Message} ].", "Error creating folder", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                return Path.Combine(folder, "machine_settings.xml");
            }
        }

        static public bool Save(Machine machine)
        {
            return Save(machine, AppDataPersistence.MachineDataPath);
        }
        static public bool Save(Machine machine, string filename)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(Machine));
                TextWriter tw = new StreamWriter(filename);
                xs.Serialize(tw, machine);

                //using (Stream stream = File.Open(filename, FileMode.Create))
                //{
                //    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                //    binaryFormatter.Serialize(stream, machine);
                //}

                tw.Close();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was an error: [ {ex.Message} ].", "Error while saving the configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        static public Machine TryLoad()
        {
            return TryLoad(AppDataPersistence.MachineDataPath);
        }
        static public Machine TryLoad(string filename)
        {
            if (!File.Exists(filename))
            {
                try
                {
                    using (var sr = new StreamReader(filename))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(Machine));
                        Machine loaded =  (Machine)xs.Deserialize(sr);

                        Machine machine = new Machine();

                        foreach (Installation i in loaded.Installations)
                        {
                            // we should re-discover them again, they should not be persistent
                            if (i.IsAutodiscoveredInstance)
                                continue;

                            if (String.IsNullOrEmpty(i.Path))
                                continue;

                            Installation new_installation;

                            if (i.java_home_instance)
                                new_installation = new Installation(true);
                            else
                                new_installation = new Installation(i.Path);

                            // we need to set these up after initialization to override constructor/propertychanged logic
                            // except for autodiscovered instances
                            if (!i.IsAutodiscoveredInstance) // normally this should always be triggered
                            {
                                new_installation.WatchedRelease = i.WatchedRelease;
                                new_installation.JVM_Implementation = i.JVM_Implementation;
                                new_installation.ImageType = i.ImageType;
                                new_installation.Arch = i.Arch;
                                new_installation.HeapSize = i.HeapSize;
                                new_installation.SkippedReleaseName = i.SkippedReleaseName;
                                new_installation.CheckForUpdatesFlag = i.CheckForUpdatesFlag;
                            }

                            machine.Installations.Add(new_installation);
                        }

                        machine.ActivateAutoDiscoveryOnPropertyChange();

                        machine.DiscoverMachineWideInstallations = loaded.DiscoverMachineWideInstallations;
                        machine.DiscoverUserScopeInstallations = loaded.DiscoverUserScopeInstallations;
                        machine.ShowShadowedInstallations = loaded.ShowShadowedInstallations;

                        foreach (Installation ai in loaded.Installations.Where(x => x.IsAutodiscoveredInstance && x.HasSkippedRelease))
                        {
                            (machine.Installations.Where(m => m.IsAutodiscoveredInstance && m.Path == ai.Path).First()).SkippedReleaseName = ai.SkippedReleaseName;
                        }

                        machine.SomethingHasBeenChangedSinceUpdateCheck = false;

                        return machine;
                    }

                    //using (Stream stream = File.Open(filename, FileMode.Open))
                    //{
                    //    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    //    return (Machine)binaryFormatter.Deserialize(stream);
                    //}

                }
                catch (Exception ex) { MessageBox.Show($"There was an error: [ {ex.Message} ].", "Error while loading the configuration", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            // quasi - else:
            Debug.WriteLine($"Machine settings [{filename}] not found, loading default machine with auto-discovery enabled.");

            // create new Machine with default settings
            Machine default_machine = new Machine();

            default_machine.ActivateAutoDiscoveryOnPropertyChange(); // not needed now, required for further changes
            default_machine.RefreshAutoDiscoveredInstallations(); // actually try to find the installations

            return default_machine;            

        }
    }
}
