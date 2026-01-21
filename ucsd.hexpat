// https://github.com/WerWolv/ImHex
// https://docs.werwolv.net/pattern-language/
// https://gbatemp.net/threads/unicorn-overlord-save-editing.650584/

// For auto-loading, place or symlink it under:
// `%appdata%/../Local/imhex/patterns/` or `~/.config/imhex/patterns/`

// UCSD
#pragma magic [ 55 43 53 44 ] @ 0x04
#pragma endian little

import std.io;
import std.core;
import std.limits as limits;


bool show_empty_entries in;


struct State {
    u32 gold @ 0x20;
    u32 renown @ 0x24;
    u16 zenoira_difficulty @ 0x4DA39E; // value is 0x4040 if set
} [[static, inline]];

struct InventoryItem {
    u32 id;
    u32 index;
    u24 count;
    u8 equip_slot_index;
    u32 equipped_icon_id; // Icon ID of who has it equipped
    u32 status; // Arbitrary item status
} [[static, fixed_size(20)]];

struct Character {
    u32 id;
    padding[28];
    u32 unit_position; // Starting 0 from top-right, FF if not in any Unit
    padding[4];
    u8 class;
    padding[9];
    bool _voice_not_editable; // FF if true?
    u8 _voice;
    u32 name;
    u32 exp;
    u16 level;
    u16 hp;
    u8 hp_plus;
    u8 attack_plus;
    u8 defense_plus;
    u8 magic_attack_plus;
    u8 magic_defense_plus;
    u8 hit_rate_plus;
    u8 evasion_plus;
    u8 critical_plus;
    u8 guard_plus;
    u8 speed_plus;
    u8;
    u8;
    u32 item_index[4];
    
    if (id == limits::u32_max() && !show_empty_entries)
    // Doesn't work for static type :(
    // Can't have common colors
        continue; // Using continue as break seems to include the last pattern it's stopping at
} [[fixed_size(0x1D0)]];

struct RapportItem {
    u32 to_char_id;
    u16 bond;
    u16 stage;
    
    if (to_char_id == limits::u32_max() && !show_empty_entries)
        continue;
} [[fixed_size(8)]];

struct Rapport {
    u32 from_char_id;
    RapportItem rapports[164];
    
    if (from_char_id == limits::u32_max() && !show_empty_entries)
        continue;
} [[fixed_size(0x524)]];

struct Unit {
    u8 capacity;
    padding[1669];
    bool enabled;
} [[static, fixed_size(1720)]];


State state @ 0x00;
InventoryItem inventory[3800] @ 0xA0;
Character characters[500] @ 0x2AF40;
Rapport rapports[164] @ 0x1B5830;
Unit units[10] @ 0x10D89A;