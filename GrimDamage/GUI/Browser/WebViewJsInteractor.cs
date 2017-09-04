﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrimDamage.Statistics.dto;
using GrimDamage.Tracking.Model;
using Newtonsoft.Json;

namespace GrimDamage.GUI.Browser {
    public class WebViewJsInteractor {
        private readonly JsonSerializerSettings _settings;

        public WebViewJsInteractor() {
            _settings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
        }

        public void SetDamageTaken(Dictionary<int, List<DamageEntryJson>> value) {
            damageTakenJson = JsonConvert.SerializeObject(value, _settings);
        }

        public void SetDamageDealt(Dictionary<int, List<DamageEntryJson>> value) {
            damageDealtJson = JsonConvert.SerializeObject(value, _settings);
        }

        public void SetPlayers(List<PlayerJson> value)  {
            playersJson = JsonConvert.SerializeObject(value, _settings);
        }

        
        public event EventHandler OnRequestUpdate;



        // FROM JS ONLY
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
        public string damageTakenJson { get; set; }

        // ReSharper disable once InconsistentNaming
        public string damageDealtJson { get; set; }

        // ReSharper disable once InconsistentNaming
        public string playersJson { get; set; }

        public void requestUpdate() {
            OnRequestUpdate?.Invoke(this, null);
        }


    }
}