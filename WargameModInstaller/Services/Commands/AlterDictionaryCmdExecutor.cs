﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Infrastructure.Dictionaries;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Dictionaries;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Services.Commands
{
    public class AlterDictionaryCmdExecutor : ReplaceCmdExecutorBase<AlterDictionaryCmd>
    {
        public AlterDictionaryCmdExecutor(AlterDictionaryCmd command)
            : base(command)
        {
            this.TotalSteps = 2;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null)
        {
            CurrentStep = 0;
            CurrentMessage = Command.GetExecutionMessage();

            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            String targetfullPath = Command.TargetPath.GetAbsoluteOrPrependIfRelative(context.InstallerTargetDirectory);
            if (!File.Exists(targetfullPath))
            {
                throw new CmdExecutionFailedException(
                    "Command's Target paths is not a valid file path.",
                    String.Format(Properties.Resources.AlterDictionartErrorMsg));
            }

            String contentPath = Command.TargetContentPath.LastPart;
            if (contentPath == null)
            {
                throw new CmdExecutionFailedException(
                    "Invalid command's TargetContentPath value.",
                    String.Format(Properties.Resources.AlterDictionartErrorMsg));
            }

            var edataFileReader = new EdataFileReader();
            var contentOwningEdata = CanGetEdataFromContext(context) ?
                GetEdataFromContext(context) :
                edataFileReader.Read(targetfullPath, false);

            EdataContentFile contentFile = contentOwningEdata.GetContentFileByPath(contentPath);
            if (!contentFile.IsContentLoaded)
            {
                edataFileReader.LoadContent(contentFile);
            }

            CurrentStep++;

            var dictReader = new DictionaryBinReader();
            var entries = dictReader.Read(contentFile.Content);
            var hashToEntriesMap = entries.ToDictionary(key => key.Hash, new ByteArrayComparer());

            foreach (var alteredEntry in Command.AlteredEntries)
            {
                var hash = MiscUtilities.HexByteStringToByteArray(alteredEntry.Key);
                DictionaryEntry entry;
                if (hashToEntriesMap.TryGetValue(hash, out entry))
                {
                    entry.Content = alteredEntry.Value;
                }
                else
                {
                    var warning = String.Format("The entry with the given hash \"{0}\" wasn't found.", alteredEntry.Key);
                    Common.Logging.LoggerFactory.Create(this.GetType()).Warn(warning);
                }
            }

            var dictWriter = new DictionaryBinWriter();
            var rawDictionaryData = dictWriter.Write(entries);
            contentFile.Content = rawDictionaryData;

            if (!CanGetEdataFromContext(context))
            {
                IEdataFileWriter edataWriter = new EdataFileWriter();
                if (token.HasValue)
                {
                    edataWriter.Write(contentOwningEdata, token.Value);
                }
                else
                {
                    edataWriter.Write(contentOwningEdata);
                }
            }

            CurrentStep = TotalSteps;
        }


    }
}