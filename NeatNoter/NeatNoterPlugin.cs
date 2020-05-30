﻿using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.IO;
using System.Linq;
using Lumina.Excel.GeneratedSheets;
using NeatNoter.Models;

namespace NeatNoter
{
    public class NeatNoterPlugin : IDalamudPlugin, IMapProvider
    {
        private DalamudPluginInterface pluginInterface;
        private NeatNoterConfiguration config;
        private NeatNoterUI ui;
        private Notebook notebook;

        public string Name => "NeatNoter";

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;

            this.config = (NeatNoterConfiguration)this.pluginInterface.GetPluginConfig() ?? new NeatNoterConfiguration();
            this.config.Initialize(this.pluginInterface);

            this.notebook = new Notebook(this.config);

            this.ui = new NeatNoterUI(this.notebook, this.config, this);
            this.pluginInterface.UiBuilder.OnBuildUi += this.ui.Draw;
            
            AddComandHandlers();
        }

        private void ToggleNotebook(string command, string args)
        {
            this.ui.IsVisible = !this.ui.IsVisible;
        }

        private void AddComandHandlers()
        {
            this.pluginInterface.CommandManager.AddHandler("/notebook", new CommandInfo(ToggleNotebook)
            {
                HelpMessage = "Open/close the NeatNoter notebook.",
                ShowInHelp = true,
            });
        }

        private void RemoveCommandHandlers()
        {
            this.pluginInterface.CommandManager.RemoveHandler("/notebook");
        }

        public MemoryStream GetCurrentMap()
        {
            if (!this.pluginInterface.Data.IsDataReady || this.pluginInterface.ClientState.LocalPlayer == null)
                return new MemoryStream();

            var currentTerritory = this.pluginInterface.ClientState.TerritoryType;
            var currentMap = this.pluginInterface.Data.GetExcelSheet<Map>()
                .GetRows()
                .FirstOrDefault(row => row.TerritoryType == currentTerritory);
            if (currentMap == null)
                return new MemoryStream();
            return this.pluginInterface.Data.GetFile(currentMap.Id).FileStream;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveCommandHandlers();
                
                this.pluginInterface.SavePluginConfig(this.config);

                this.pluginInterface.UiBuilder.OnBuildUi -= this.ui.Draw;
                this.ui.Dispose();

                this.pluginInterface.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
