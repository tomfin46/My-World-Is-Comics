﻿namespace MyWorldIsComics.DataModel.DescriptionContent
{
    internal class Description
    {
        public int UniqueId { get; set; }
        public Section CurrentEvents { get; set; }
        public Section Origin { get; set; }
        public Section Creation { get; set; }
        public Section CharacterEvolution { get; set; }
        public Section MajorStoryArcs { get; set; }
        public Section AlternateRealities { get; set; }
        public Section DistinguishingCharacteristics { get; set; }
        public Section PowersAndAbilities { get; set; }
        public Section OtherMedia { get; set; }
        public Section WeaponsAndEquipment { get; set; }
    }
}