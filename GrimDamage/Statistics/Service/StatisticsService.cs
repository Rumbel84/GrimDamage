﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrimDamage.Parser.Service;
using GrimDamage.Statistics.dto;
using GrimDamage.Tracking.Model;

namespace GrimDamage.Statistics.Service {
    public class StatisticsService {
        private DateTime _lastUpdateTimeDamageDealt = DateTime.UtcNow;
        private DateTime _lastUpdateTimeDamageDealtSingleTarget = DateTime.UtcNow;
        private DateTime _lastUpdateTimeDamageTaken = DateTime.UtcNow;
        private DateTime _lastUpdateTimeDetailedDamageTaken = DateTime.UtcNow;
        private DateTime _lastUpdateTimeDetailedDamageDealt = DateTime.UtcNow;
        private readonly DamageParsingService _damageParsingService;

        public StatisticsService(DamageParsingService damageParsingService) {
            _damageParsingService = damageParsingService;
        }

        private EntityJson toJson(Entity m) {
            return new EntityJson {
                Id = m.Id,
                Name = m.Name,
                IsPrimary = m.IsPrimary,
                Type = m.Type.ToString(),
                Health = m.Health
            };
        }

        public List<EntityJson> GetEntities() {
            return _damageParsingService.Values.Select(toJson).ToList();
        }

        public List<EntityJson> GetPlayers() {
            return _damageParsingService.Values.Where(entity => entity.Type == EntityType.Player).Select(toJson).ToList();
        }

        public List<EntityJson> GetPets() {
            return _damageParsingService.Values.Where(entity => entity.Type == EntityType.Pet).Select(toJson).ToList();
        }

        private List<SimpleDamageEntryJson> Normalize(List<SimpleDamageEntryJson> entries) {
            foreach (DamageType type in Enum.GetValues(typeof(DamageType))) {
                if (type != DamageType.Unknown) {
                    if (!entries.Exists(m => m.DamageType == type.ToString())) {
                        entries.Add(new SimpleDamageEntryJson {
                            Amount = 0,
                            DamageType = type.ToString()
                        });
                    }
                }
            }

            entries.Add(new SimpleDamageEntryJson {
                Amount = entries.Sum(m => m.Amount),
                DamageType = "Total"
            });

            return entries;
        }

        public List<DamageBlockedJson> GetDamageBlocked(int playerId) {
            return _damageParsingService.GetEntity(playerId).DamageBlocked
                .Select(m => new DamageBlockedJson {
                    AttackerId = m.Attacker,
                    Amount = m.Amount
                })
                .ToList();
        }

        public List<DetailedDamageDealtJson> GetDetailedLatestDamageDealt(int playerId) {
            var player = _damageParsingService.GetEntity(playerId);
            if (player == null || player.DamageDealt.Count == 0) {
                return new List<DetailedDamageDealtJson>();
            }
            else {
                var result = player.DamageDealt
                    .Where(dmg => dmg.Time > _lastUpdateTimeDetailedDamageDealt)
                    .Select(m => new DetailedDamageDealtJson {
                        VictimId = m.Target,
                        DamageType = m.Type.ToString(),
                        Amount = m.Amount
                    })
                    .ToList();

                _lastUpdateTimeDetailedDamageDealt = player.DamageDealt.Max(m => m.Time);
                return result;
            }
        }

        public List<DetailedDamageTakenJson> GetDetailedLatestDamageTaken(int playerId, long timestamp) {
            var player = _damageParsingService.GetEntity(playerId);
            if (player == null || player.DamageTaken.Count == 0) {
                return new List<DetailedDamageTakenJson>();
            }
            else {
                var from = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(timestamp);
                var result = player.DamageTaken
                    .Where(dmg => dmg.Time > from)
                    .Select(m => new DetailedDamageTakenJson {
                        AttackerId = m.Attacker,
                        DamageType = m.Type.ToString(),
                        Amount = m.Amount
                    })
                    .ToList();

                return result;
            }
        }

        public List<DetailedDamageTakenJson> GetDetailedLatestDamageTaken(int playerId) {
            var player = _damageParsingService.GetEntity(playerId);
            if (player == null || player.DamageTaken.Count == 0) {
                return new List<DetailedDamageTakenJson>();
            }
            else {
                var result = player.DamageTaken
                    .Where(dmg => dmg.Time > _lastUpdateTimeDetailedDamageTaken)
                    .Select(m => new DetailedDamageTakenJson {
                        AttackerId = m.Attacker,
                        DamageType = m.Type.ToString(),
                        Amount = m.Amount
                    })
                    .ToList();

                _lastUpdateTimeDetailedDamageTaken = player.DamageTaken.Max(m => m.Time);
                return result;
            }
        }

        public List<SimpleDamageEntryJson> GetLatestDamageTaken(int playerId) {
            var player = _damageParsingService.GetEntity(playerId);

            if (player == null || player.DamageTaken.Count == 0) {
                return Normalize(new List<SimpleDamageEntryJson>());
            }
            else {
                var result = player.DamageTaken
                    .Where(dmg => dmg.Time > _lastUpdateTimeDamageTaken)
                    .GroupBy(m => m.Type)
                    .Select(m => new SimpleDamageEntryJson {
                        DamageType = m.Key.ToString(),
                        Amount = m.Sum(s => s.Amount)
                    })
                    .ToList();

                _lastUpdateTimeDamageTaken = player.DamageTaken.Max(m => m.Time);
                return Normalize(result);
            }
        }

        public List<SimpleDamageEntryJson> GetLatestDamageDealt(int playerId) {
            var player = _damageParsingService.GetEntity(playerId);

            if (player == null || player.DamageDealt.Count == 0) {
                return Normalize(new List<SimpleDamageEntryJson>());
            }
            else {
                var result = player.DamageDealt
                    .Where(dmg => dmg.Time > _lastUpdateTimeDamageDealt)
                    .GroupBy(m => m.Type)
                    .Select(m => new SimpleDamageEntryJson {
                        DamageType = m.Key.ToString(),
                        Amount = m.Sum(s => s.Amount)
                    })
                    .ToList();

                _lastUpdateTimeDamageDealt = player.DamageDealt.Max(m => m.Time);
                return Normalize(result);
            }
        }
        
        public List<SimpleDamageEntryJson> GetLatestDamageDealtToSingleTarget(int playerId) {
            var player = _damageParsingService.GetEntity(playerId);

            if (player == null || player.DamageDealt.Count == 0) {
                return Normalize(new List<SimpleDamageEntryJson>());
            }
            else {
                var result = player.DamageDealt
                    .Where(dmg => dmg.Time > _lastUpdateTimeDamageDealtSingleTarget)
                    .GroupBy(m => m.Target)
                    .OrderByDescending(m => m.Sum(e => e.Amount))
                    .FirstOrDefault()
                    ?.GroupBy(m => m.Type)
                    .Select(m => new SimpleDamageEntryJson {
                        DamageType = m.Key.ToString(),
                        Amount = m.Sum(s => s.Amount)
                        }
                    )
                    .ToList();

                _lastUpdateTimeDamageDealtSingleTarget = player.DamageDealt.Max(m => m.Time);

                if (result == null)
                    return Normalize(new List<SimpleDamageEntryJson>());

                return Normalize(result);
            }
        }
    }
}
