﻿// Patch_GenerateImpliedDefs_PreResolve.cs
// Copyright Karel Kroeze, 2017-2017

using System.Linq;
using Harmony;
using RimWorld;
using static AnimalTab.PawnColumnDefOf;
using static AnimalTab.Constants;

namespace AnimalTab
{
    [HarmonyPatch( typeof( DefGenerator ), "GenerateImpliedDefs_PreResolve" )]
    public class Patch_GenerateImpliedDefs_PreResolve
    {
        private static void Postfix()
        {
            var animalTable = PawnTableDefOf.Animals;
            var columns = animalTable.columns;

            // replace label column
            // NOTE: We can't simply replace the workerType, as these columns are used in multiple tabs.
            var labelIndex = columns.IndexOf( Label );
            columns.RemoveAt( labelIndex );
            columns.Insert( labelIndex, AnimalTabLabel );
            var areaIndex = columns.IndexOf( AllowedAreaWide );
            columns.RemoveAt( areaIndex );
            columns.Insert( areaIndex, AnimalTabAllowedArea );

            // move master and follow columns after lifestage
            var lifeStageIndex = columns.FindIndex( c => c.workerClass == typeof( PawnColumnWorker_LifeStage ) );
            var masterColumn = columns.Find( c => c == Master );
            var followDraftedColumn = columns.Find( c => c == FollowDrafted );
            var followFieldworkColumn = columns.Find( c => c == FollowFieldwork );
            columns.Remove( masterColumn );
            columns.Insert( lifeStageIndex + 1, masterColumn );
            columns.Remove( followDraftedColumn );
            columns.Insert( lifeStageIndex + 2, followDraftedColumn );
            columns.Remove( followFieldworkColumn );
            columns.Insert( lifeStageIndex + 3, followFieldworkColumn );

            // insert wool, milk & meat columns before slaughter
            var slaughterIndex = columns.IndexOf( Slaughter );
            columns.Insert( slaughterIndex, Meat );
            columns.Insert( slaughterIndex, Milk );
            columns.Insert( slaughterIndex, Wool );

            // remove all gaps, insert new ones at appropriate places
            columns.RemoveAll( c => c == GapTiny );
            columns.Insert( lifeStageIndex + 1, GapTiny );
            columns.Insert( columns.IndexOf( followFieldworkColumn ) + 1, GapTiny );
            columns.Insert( columns.FindLastIndex( c => c.workerClass == typeof( PawnColumnWorker_Trainable ) ) + 1,
                GapTiny );
            columns.Insert( slaughterIndex + 1, GapTiny );

            // make all icons the same size.
            foreach ( var column in columns )
                column.headerIconSize = HeaderIconSize;

            // set new worker for master, slaughter, follow and pregnant columns
            Master.workerClass = typeof( PawnColumnWorker_Master );
            Slaughter.workerClass = typeof( PawnColumnWorker_Slaughter );
            FollowDrafted.workerClass = typeof( PawnColumnWorker_FollowDrafted );
            FollowFieldwork.workerClass = typeof( PawnColumnWorker_FollowFieldwork );
            Pregnant.workerClass = typeof( PawnColumnWorker_Pregnant );

            // set new workers for trainable columns
            foreach ( var column in columns.Where( c => c.workerClass == typeof( RimWorld.PawnColumnWorker_Trainable ) )
            )
                column.workerClass = typeof( PawnColumnWorker_Trainable );

            // reset all workers to make sure they are resolved to the new type
            // NOTE: Vanilla inserts columns by checking for 'Worker', which resolves some workers - we need to reset that.
            var workerField = AccessTools.Field( typeof( PawnColumnDef ), "workerInt" );
            foreach ( var column in columns )
                workerField.SetValue( column, null );

            foreach ( var column in columns )
                Logger.Debug(
                    $"{column.defName} <{column.workerClass.FullName}> | <{column.Worker?.GetType().FullName ?? "NULL"}>" );
        }
    }
}