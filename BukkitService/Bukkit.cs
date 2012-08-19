using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using BukkitServiceAPI;

namespace BukkitService {
    public partial class Bukkit : ServiceBase {

        public Bukkit() {
            InitializeComponent();
        }

        public void Start() {
            OnStart(null);
        }

        protected override void OnStart(string[] args) {
            Main.Start();
        }

        protected override void OnStop() {
            Main.Stop();
        }
    }
}
