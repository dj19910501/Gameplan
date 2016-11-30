import Grid from 'dhtmlXGridObject';

export default function createGrid($container, $grid) {
    const grid = new Grid({
        parent: $grid.get(0),
        columns: [
            {
            label: "A",
            width: 100,
            type: "ro",
            sort: "int",
            align: "right",
        },
            {
                label: "B",
                width: 100,
                type: "ed",
                sort: "str",
                align: "left",
            }
        ]
    });

    grid.enableAutoHeight(true);

    grid.parse({
        rows: [
            { id: 0, data: [99, "0Oranges"]},
            { id: 1, data: [32, "1Apples"]},
            { id: 2, data: [99, "2Oranges"]},
            { id: 3, data: [32, "3Apples"]},
            { id: 4, data: [99, "4Oranges"]},
            { id: 5, data: [32, "5Apples"]},
            { id: 6, data: [99, "6Oranges"]},
            { id: 7, data: [32, "7Apples"]},
            { id: 8, data: [99, "8Oranges"]},
            { id: 9, data: [32, "9Apples"]},
            { id: 10, data: [99, "10Oranges"]},
            { id: 11, data: [32, "11Apples"]},
            { id: 12, data: [99, "12Oranges"]},
            { id: 13, data: [32, "13Apples"]},
            { id: 14, data: [99, "14Oranges"]},
            { id: 15, data: [32, "15Apples"]},
            { id: 16, data: [99, "16Oranges"]},
            { id: 17, data: [32, "17Apples"]},
            { id: 18, data: [99, "18Oranges"]},
            { id: 19, data: [32, "19Apples"]},
            { id: 20, data: [99, "20Oranges"]},
            { id: 21, data: [32, "21Apples"]},
            { id: 22, data: [99, "22Oranges"]},
            { id: 23, data: [32, "23Apples"]},
            { id: 24, data: [99, "24Oranges"]},
            { id: 25, data: [32, "25Apples"]},
            { id: 26, data: [99, "26Oranges"]},
            { id: 27, data: [32, "27Apples"]},
            { id: 28, data: [99, "28Oranges"]},
            { id: 29, data: [32, "29Apples"]},
        ]
    }, "json");
}
