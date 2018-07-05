﻿using DevZest.Data.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Views.Primitives
{
    public class Sorting : Model
    {
        public static DataSet<Sorting> Convert(Model model, IReadOnlyList<IColumnComparer> orderBy)
        {
            model.VerifyNotNull(nameof(model));

            var result = DataSet<Sorting>.New();
            if (orderBy == null || orderBy.Count == 0)
                return result;

            for (int i = 0; i < orderBy.Count; i++)
            {
                var orderByItem = orderBy.VerifyNotNull(i, nameof(orderBy));
                result.AddRow((_, dataRow) =>
                {
                    var column = orderByItem.GetColumn(model);
                    _.Column[dataRow] = column;
                    _.Direction[dataRow] = orderByItem.Direction;
                });
            }

            return result;
        }

        public static IReadOnlyList<IColumnComparer> Convert(DataSet<Sorting> sortings)
        {
            sortings.VerifyNotNull(nameof(sortings));
            if (sortings.Validate().Count != 0)
                throw new ArgumentException(DiagnosticMessages.Sorting_HasValidationError, nameof(sortings));

            if (sortings.Count == 0)
                return Array.Empty<IColumnComparer>();

            var _ = sortings._;
            var result = new IColumnComparer[sortings.Count];
            for (int i = 0; i < sortings.Count; i++)
            {
                var column = _.Column[i];
                var direction = _.Direction[i];
                Debug.Assert(direction == SortDirection.Ascending || direction == SortDirection.Descending);
                result[i] = DataRow.OrderBy(column, direction);
            }

            return result;
        }

        static Sorting()
        {
            RegisterLocalColumn((Sorting _) => _.Column);
            RegisterLocalColumn((Sorting _) => _.Direction);
        }

        [Display(Name = nameof(UserMessages.Sorting_SortBy), ResourceType = typeof(UserMessages))]
        public LocalColumn<Column> Column { get; private set; }

        [Display(Name = nameof(UserMessages.Sorting_Order), ResourceType = typeof(UserMessages))]
        public LocalColumn<SortDirection> Direction { get; private set; }

        [ModelValidator]
        private DataValidationError ValidateRequiredColumn(DataRow dataRow)
        {
            return Column[dataRow] == null
                ? new DataValidationError(UserMessages.Sorting_InputRequired(Column.DisplayName), Column)
                : null;
        }

        [ModelValidator]
        private DataValidationError ValidateDuplicateColumn(DataRow dataRow)
        {
            var dataSet = DataSet;
            foreach (var other in dataSet)
            {
                if (other == dataRow)
                    continue;
                if (Column[dataRow] == Column[other])
                    return new DataValidationError(UserMessages.Sorting_DuplicateSortBy, Column);
            }
            return null;
        }

        [ModelValidator]
        private DataValidationError ValidateDirection(DataRow dataRow)
        {
            return Direction[dataRow] == SortDirection.Unspecified
                ? new DataValidationError(UserMessages.Sorting_InputRequired(Direction.DisplayName), Direction)
                : null;
        }
    }
}
